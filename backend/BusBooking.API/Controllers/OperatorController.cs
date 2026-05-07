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

/// <summary>
/// Operator Controller - Handles all operator-specific operations
/// </summary>
[ApiController]
[Route("api/[controller]")]
[Authorize]
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
        command.OperatorId = GetUserId();
        var result = await _mediator.Send(command);
        return Ok(new { id = result, message = "Bus registration request sent. Waiting for admin approval." });
    }

    [HttpGet("my-buses")]
    public async Task<ActionResult> GetMyBuses()
    {
        var userId = GetUserId();
        
        var busOperator = await _context.BusOperators
            .FirstOrDefaultAsync(o => o.UserId == userId);
            
        if (busOperator == null) 
        {
            return Ok(new List<object>());
        }

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

        return Ok(buses);
    }

    [HttpGet("bookings")]
    public async Task<ActionResult> GetMyBookings()
    {
        var userId = GetUserId();
        
        var busOperator = await _context.BusOperators
            .FirstOrDefaultAsync(o => o.UserId == userId);
            
        if (busOperator == null) 
        {
            return Ok(new List<object>());
        }

        var result = await _mediator.Send(new GetOperatorBookingsQuery(busOperator.Id));
        return Ok(result);
    }

    [HttpPost("schedule-trip")]
    public async Task<ActionResult> ScheduleTrip([FromBody] ScheduleTripCommand command)
    {
        var userId = GetUserId();
        
        var busOperator = await _context.BusOperators
            .FirstOrDefaultAsync(o => o.UserId == userId);
            
        if (busOperator == null) 
        {
            throw new UnauthorizedAccessException("Operator profile not found");
        }

        var bus = await _context.Buses
            .FirstOrDefaultAsync(b => b.Id == command.BusId && b.OperatorId == busOperator.Id);

        if (bus == null)
        {
            throw new UnauthorizedAccessException("Bus not found or does not belong to you");
        }

        var tripId = await _mediator.Send(command);
        return Ok(new { id = tripId, message = "Trip scheduled successfully!" });
    }

    [HttpGet("buses/{busId}/trips")]
    public async Task<ActionResult> GetBusTrips(Guid busId)
    {
        var userId = GetUserId();
        
        var busOperator = await _context.BusOperators
            .FirstOrDefaultAsync(o => o.UserId == userId);
            
        if (busOperator == null) 
        {
            throw new UnauthorizedAccessException("Operator profile not found");
        }

        var bus = await _context.Buses
            .FirstOrDefaultAsync(b => b.Id == busId && b.OperatorId == busOperator.Id);

        if (bus == null)
        {
            throw new UnauthorizedAccessException("Bus not found or does not belong to you");
        }

        var result = await _mediator.Send(new GetBusTripsQuery(busId));
        return Ok(result);
    }

    [HttpGet("notifications")]
    public async Task<ActionResult> GetNotifications([FromQuery] bool unreadOnly = false)
    {
        var userId = GetUserId();
        var result = await _mediator.Send(new BusBooking.Application.Notifications.Queries.GetOperatorNotifications.GetOperatorNotificationsQuery(userId, unreadOnly));
        return Ok(result);
    }

    [HttpPut("notifications/{notificationId}/mark-read")]
    public async Task<ActionResult> MarkNotificationAsRead(Guid notificationId)
    {
        var userId = GetUserId();
        var result = await _mediator.Send(new BusBooking.Application.Notifications.Commands.MarkNotificationAsRead.MarkNotificationAsReadCommand(notificationId, userId));
        
        if (!result)
        {
            throw new KeyNotFoundException($"Notification with ID {notificationId} not found");
        }
        
        return Ok(new { message = "Notification marked as read" });
    }

    [HttpGet("dashboard/stats")]
    public async Task<ActionResult> GetDashboardStats()
    {
        var userId = GetUserId();
        
        var busOperator = await _context.BusOperators
            .FirstOrDefaultAsync(o => o.UserId == userId);
            
        if (busOperator == null) 
        {
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
        return Ok(result);
    }
}