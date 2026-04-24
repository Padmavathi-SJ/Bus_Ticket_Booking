using BusBooking.Application.Bookings.Queries.GetOperatorBookings;
using BusBooking.Application.Buses.Commands.CreateBus;
using BusBooking.Application.Routes.Queries.GetRoutes;
using BusBooking.Application.Trips.Commands.ScheduleTrip;
using BusBooking.Application.Trips.Queries.GetBusTrips;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace BusBooking.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize] // Use internal check if Roles="BusOperator" is too strict due to claim mapping
public class OperatorController : ControllerBase
{
    private readonly IMediator _mediator;
    private readonly BusBooking.Application.Common.Interfaces.IAppDbContext _context;

    public OperatorController(IMediator mediator, BusBooking.Application.Common.Interfaces.IAppDbContext context)
    {
        _mediator = mediator;
        _context = context;
    }

    private Guid GetUserId()
    {
        var claim = User.FindFirst(ClaimTypes.NameIdentifier) ?? 
                    User.FindFirst("sub") ?? 
                    User.FindFirst("id");

        if (claim == null)
        {
            var allClaims = string.Join(", ", User.Claims.Select(c => $"{c.Type}:{c.Value}"));
            throw new UnauthorizedAccessException($"User ID not found in token. Available claims: {allClaims}");
        }
        
        return Guid.Parse(claim.Value);
    }

    [HttpGet("routes")]
    public async Task<ActionResult> GetRoutes()
    {
        var result = await _mediator.Send(new GetRoutesQuery());
        return Ok(result);
    }

    [HttpPost("add-bus")]
    public async Task<ActionResult> AddBus([FromBody] CreateBusCommand command)
    {
        command.OperatorId = GetUserId(); // Set from session
        var result = await _mediator.Send(command);
        return Ok(new { id = result, message = "Bus registration request sent. Waiting for admin approval." });
    }

    [HttpGet("my-buses")]
    public async Task<ActionResult> GetMyBuses()
    {
        var userId = GetUserId();
        Console.WriteLine($"[DEBUG] Fetching buses for UserID: {userId}");
        
        // Find the operator record
        var busOperator = await _context.BusOperators
            .FirstOrDefaultAsync(o => o.UserId == userId);
            
        if (busOperator == null) 
        {
            Console.WriteLine($"[DEBUG] No BusOperator profile found for UserID: {userId}");
            return Ok(new List<object>());
        }

        Console.WriteLine($"[DEBUG] Found BusOperator profile: {busOperator.Id} for Company: {busOperator.CompanyName}");

        // Fetch buses belonging to this operator
        var buses = await _context.Buses
            .Include(b => b.Route)
            .Where(b => b.OperatorId == busOperator.Id)
            .OrderByDescending(b => b.CreatedAt)
            .Select(b => new {
                b.Id,
                b.BusName,
                b.BusNumber,
                b.BusType,
                b.TotalSeats,
                b.FemaleSeats,
                b.MaleSeats,
                b.RouteId,
                b.Status,
                b.IsAvailable,
                b.RejectionReason,
                RouteName = b.Route != null ? b.Route.Name : "No Route assigned"
            })
            .ToListAsync();

        Console.WriteLine($"[DEBUG] Found {buses.Count} buses for OperatorID: {busOperator.Id}");

        return Ok(buses);
    }

    [HttpGet("bookings")]
    public async Task<ActionResult> GetMyBookings()
    {
        try
        {
            var userId = GetUserId();
            Console.WriteLine($"[DEBUG] Fetching bookings for UserID: {userId}");
            
            // Find the operator record
            var busOperator = await _context.BusOperators
                .FirstOrDefaultAsync(o => o.UserId == userId);
                
            if (busOperator == null) 
            {
                Console.WriteLine($"[DEBUG] No BusOperator profile found for UserID: {userId}");
                return Ok(new List<object>());
            }

            Console.WriteLine($"[DEBUG] Found BusOperator profile: {busOperator.Id}");

            var result = await _mediator.Send(new GetOperatorBookingsQuery(busOperator.Id));
            Console.WriteLine($"[DEBUG] Found {result.Count} bookings for OperatorID: {busOperator.Id}");
            
            return Ok(result);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[ERROR] Failed to fetch operator bookings: {ex.Message}");
            return BadRequest(new { message = ex.Message });
        }
    }

    [HttpPost("schedule-trip")]
    public async Task<ActionResult> ScheduleTrip([FromBody] ScheduleTripCommand command)
    {
        try
        {
            var userId = GetUserId();
            Console.WriteLine($"[DEBUG] Scheduling trip for UserID: {userId}, BusID: {command.BusId}");
            
            // Verify the bus belongs to this operator
            var busOperator = await _context.BusOperators
                .FirstOrDefaultAsync(o => o.UserId == userId);
                
            if (busOperator == null) 
            {
                return Unauthorized(new { message = "Operator profile not found" });
            }

            var bus = await _context.Buses
                .FirstOrDefaultAsync(b => b.Id == command.BusId && b.OperatorId == busOperator.Id);

            if (bus == null)
            {
                return Unauthorized(new { message = "Bus not found or does not belong to you" });
            }

            var tripId = await _mediator.Send(command);
            Console.WriteLine($"[DEBUG] Trip scheduled successfully with ID: {tripId}");
            
            return Ok(new { id = tripId, message = "Trip scheduled successfully!" });
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[ERROR] Failed to schedule trip: {ex.Message}");
            return BadRequest(new { message = ex.Message });
        }
    }

    [HttpGet("buses/{busId}/trips")]
    public async Task<ActionResult> GetBusTrips(Guid busId)
    {
        try
        {
            var userId = GetUserId();
            Console.WriteLine($"[DEBUG] Fetching trips for BusID: {busId}, UserID: {userId}");
            
            // Verify the bus belongs to this operator
            var busOperator = await _context.BusOperators
                .FirstOrDefaultAsync(o => o.UserId == userId);
                
            if (busOperator == null) 
            {
                return Unauthorized(new { message = "Operator profile not found" });
            }

            var bus = await _context.Buses
                .FirstOrDefaultAsync(b => b.Id == busId && b.OperatorId == busOperator.Id);

            if (bus == null)
            {
                return Unauthorized(new { message = "Bus not found or does not belong to you" });
            }

            var result = await _mediator.Send(new GetBusTripsQuery(busId));
            Console.WriteLine($"[DEBUG] Found {result.Count} trips for BusID: {busId}");
            
            return Ok(result);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[ERROR] Failed to fetch bus trips: {ex.Message}");
            return BadRequest(new { message = ex.Message });
        }
    }

    [HttpGet("notifications")]
    public async Task<ActionResult> GetNotifications([FromQuery] bool unreadOnly = false)
    {
        try
        {
            var userId = GetUserId();
            Console.WriteLine($"[DEBUG] Fetching notifications for UserID: {userId}, UnreadOnly: {unreadOnly}");
            
            var result = await _mediator.Send(new BusBooking.Application.Notifications.Queries.GetOperatorNotifications.GetOperatorNotificationsQuery(userId, unreadOnly));
            Console.WriteLine($"[DEBUG] Found {result.Count} notifications");
            
            return Ok(result);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[ERROR] Failed to fetch notifications: {ex.Message}");
            return BadRequest(new { message = ex.Message });
        }
    }

    [HttpPut("notifications/{notificationId}/mark-read")]
    public async Task<ActionResult> MarkNotificationAsRead(Guid notificationId)
    {
        try
        {
            var userId = GetUserId();
            Console.WriteLine($"[DEBUG] Marking notification {notificationId} as read for UserID: {userId}");
            
            var result = await _mediator.Send(new BusBooking.Application.Notifications.Commands.MarkNotificationAsRead.MarkNotificationAsReadCommand(notificationId, userId));
            
            if (!result)
            {
                return NotFound(new { message = "Notification not found" });
            }
            
            return Ok(new { message = "Notification marked as read" });
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[ERROR] Failed to mark notification as read: {ex.Message}");
            return BadRequest(new { message = ex.Message });
        }
    }

    [HttpGet("dashboard/stats")]
    public async Task<ActionResult> GetDashboardStats()
    {
        try
        {
            var userId = GetUserId();
            Console.WriteLine($"[DEBUG] Fetching dashboard stats for UserID: {userId}");
            
            // Find the operator record
            var busOperator = await _context.BusOperators
                .FirstOrDefaultAsync(o => o.UserId == userId);
                
            if (busOperator == null) 
            {
                Console.WriteLine($"[DEBUG] No BusOperator profile found for UserID: {userId}");
                return Ok(new
                {
                    TotalBuses = 0,
                    ActiveBuses = 0,
                    PendingBuses = 0,
                    TotalTrips = 0,
                    UpcomingTrips = 0,
                    TotalBookings = 0,
                    TodayBookings = 0,
                    TotalSeatsBooked = 0,
                    TotalRevenue = 0m,
                    TodayRevenue = 0m
                });
            }

            var result = await _mediator.Send(new BusBooking.Application.Operator.Queries.GetOperatorDashboardStats.GetOperatorDashboardStatsQuery(busOperator.Id));
            Console.WriteLine($"[DEBUG] Dashboard stats retrieved for OperatorID: {busOperator.Id}");
            
            return Ok(result);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[ERROR] Failed to fetch dashboard stats: {ex.Message}");
            return BadRequest(new { message = ex.Message });
        }
    }
}
