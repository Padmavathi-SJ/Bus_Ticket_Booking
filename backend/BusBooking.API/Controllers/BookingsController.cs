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

/// <summary>
/// Bookings Controller - Handles all booking operations
/// </summary>
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
        var result = await _mediator.Send(new GetBookedSeatsForTripQuery(tripId));
        return Ok(result);
    }

    /// <summary>
    /// Create a new booking
    /// </summary>
    [HttpPost]
    [Authorize]
    public async Task<ActionResult<BookingConfirmationDto>> CreateBooking([FromBody] CreateBookingDto dto)
    {
        var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (string.IsNullOrEmpty(userId))
        {
            throw new UnauthorizedAccessException("User not authenticated");
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

    /// <summary>
    /// Get all bookings for the logged-in user
    /// </summary>
    [HttpGet("my-bookings")]
    [Authorize]
    public async Task<ActionResult<List<BookingDto>>> GetMyBookings()
    {
        var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (string.IsNullOrEmpty(userId))
        {
            throw new UnauthorizedAccessException("User not authenticated");
        }

        var result = await _mediator.Send(new GetUserBookingsQuery(Guid.Parse(userId)));
        return Ok(result);
    }

    /// <summary>
    /// Cancel a booking
    /// </summary>
    [HttpPut("{id}/cancel")]
    [Authorize]
    public async Task<ActionResult<CancelBookingResult>> CancelBooking(Guid id)
    {
        var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (string.IsNullOrEmpty(userId))
        {
            throw new UnauthorizedAccessException("User not authenticated");
        }

        var result = await _mediator.Send(new CancelBookingCommand(id, Guid.Parse(userId)));
        return Ok(result);
    }
}