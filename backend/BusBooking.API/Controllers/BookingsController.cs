using BusBooking.Application.Bookings.Commands.CancelBooking;
using BusBooking.Application.Bookings.Commands.CreateBooking;
using BusBooking.Application.Bookings.DTOs;
using BusBooking.Application.Bookings.Queries.GetBookedSeats;
using BusBooking.Application.Bookings.Queries.GetBookedSeatsForTrip;
using BusBooking.Application.Bookings.Queries.GetUserBookings;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace BusBooking.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class BookingsController : ControllerBase
{
    private readonly IMediator _mediator;

    public BookingsController(IMediator mediator)
    {
        _mediator = mediator;
    }

    /// <summary>
    /// Get booked seats for a specific bus
    /// </summary>
    [HttpGet("booked-seats/{busId}")]
    public async Task<ActionResult<List<string>>> GetBookedSeats(Guid busId)
    {
        var result = await _mediator.Send(new GetBookedSeatsQuery(busId));
        return Ok(result);
    }

    /// <summary>
    /// Get booked seats for a specific trip
    /// </summary>
    [HttpGet("booked-seats/trip/{tripId}")]
    public async Task<ActionResult<List<string>>> GetBookedSeatsForTrip(Guid tripId)
    {
        try
        {
            Console.WriteLine($"[DEBUG-BOOKED-SEATS] Getting booked seats for trip: {tripId}");
            var result = await _mediator.Send(new GetBookedSeatsForTripQuery(tripId));
            Console.WriteLine($"[DEBUG-BOOKED-SEATS] Found {result.Count} booked seats");
            return Ok(result);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[ERROR-BOOKED-SEATS] Failed: {ex.Message}");
            return BadRequest(new { message = ex.Message });
        }
    }

    /// <summary>
    /// Create a new booking
    /// </summary>
    [HttpPost]
    [Authorize]
    public async Task<ActionResult<BookingConfirmationDto>> CreateBooking([FromBody] CreateBookingDto dto)
    {
        try
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userId))
            {
                return Unauthorized(new { message = "User not authenticated" });
            }

            var command = new CreateBookingCommand(
                Guid.Parse(userId),
                dto.BusId,
                dto.TripId,
                dto.SeatNumbers,
                dto.PassengerDetails,
                dto.TotalAmount,
                dto.PaymentMethod,
                dto.PaymentStatus
            );

            var result = await _mediator.Send(command);
            return Ok(result);
        }
        catch (Exception ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    /// <summary>
    /// Get all bookings for the logged-in user
    /// </summary>
    [HttpGet("my-bookings")]
    [Authorize]
    public async Task<ActionResult<List<BookingDto>>> GetMyBookings()
    {
        try
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userId))
            {
                Console.WriteLine("[BookingsController] User not authenticated");
                return Unauthorized(new { message = "User not authenticated" });
            }

            Console.WriteLine($"[BookingsController] Fetching bookings for user: {userId}");
            var result = await _mediator.Send(new GetUserBookingsQuery(Guid.Parse(userId)));
            Console.WriteLine($"[BookingsController] Found {result.Count} bookings for user: {userId}");
            
            return Ok(result);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[BookingsController] Error: {ex.Message}");
            Console.WriteLine($"[BookingsController] Stack trace: {ex.StackTrace}");
            return BadRequest(new { message = ex.Message });
        }
    }

    /// <summary>
    /// Cancel a booking
    /// </summary>
    [HttpPut("{id}/cancel")]
    [Authorize]
    public async Task<ActionResult<CancelBookingResult>> CancelBooking(Guid id)
    {
        try
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userId))
            {
                return Unauthorized(new { message = "User not authenticated" });
            }

            var result = await _mediator.Send(new CancelBookingCommand(id, Guid.Parse(userId)));
            return Ok(result);
        }
        catch (UnauthorizedAccessException ex)
        {
            return Forbid(ex.Message);
        }
        catch (Exception ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }
}
