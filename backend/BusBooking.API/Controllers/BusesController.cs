using BusBooking.Application.Admin.Queries.GetBuses;
using BusBooking.Application.Buses.Queries.GetBusById;
using BusBooking.Application.Trips.Queries.SearchTrips;
using BusBooking.Application.Trips.Queries.GetTripById;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace BusBooking.API.Controllers;

/// <summary>
/// Buses Controller - Public endpoints for bus and trip search
/// </summary>
[ApiController]
[Route("api/[controller]")]
public class BusesController : ControllerBase
{
    private readonly IMediator _mediator;

    public BusesController(IMediator mediator)
    {
        _mediator = mediator;
    }

    /// <summary>
    /// Get bus details by ID
    /// </summary>
    [HttpGet("{id}")]
    public async Task<ActionResult> GetBusById(Guid id)
    {
        var result = await _mediator.Send(new GetBusByIdQuery(id));
        if (result == null)
        {
            throw new KeyNotFoundException($"Bus with ID {id} not found or not available");
        }
        return Ok(result);
    }

    /// <summary>
    /// Search buses by source and destination
    /// </summary>
    [HttpGet("search")]
    public async Task<ActionResult> SearchBuses([FromQuery] string? source, [FromQuery] string? destination)
    {
        var result = await _mediator.Send(new GetBusesQuery(source, destination));
        
        var availableBuses = result.Where(b => 
            b.Status == BusBooking.Domain.Enums.BusStatus.Approved && 
            b.IsAvailable).ToList();

        if (!string.IsNullOrEmpty(source) || !string.IsNullOrEmpty(destination))
        {
            availableBuses = availableBuses.Where(b => 
                (string.IsNullOrEmpty(source) || (!string.IsNullOrEmpty(b.Source) && b.Source.Contains(source, StringComparison.OrdinalIgnoreCase))) &&
                (string.IsNullOrEmpty(destination) || (!string.IsNullOrEmpty(b.Destination) && b.Destination.Contains(destination, StringComparison.OrdinalIgnoreCase)))
            ).ToList();
        }
            
        return Ok(availableBuses);
    }

    /// <summary>
    /// Search trips by source, destination, and date
    /// </summary>
    [HttpGet("search-trips")]
    public async Task<ActionResult> SearchTrips(
        [FromQuery] string? source, 
        [FromQuery] string? destination,
        [FromQuery] string? tripDate)
    {
        DateTime? parsedDate = null;
        
        if (!string.IsNullOrEmpty(tripDate))
        {
            if (!DateTime.TryParse(tripDate, out var date))
            {
                throw new ArgumentException("Invalid date format. Use YYYY-MM-DD");
            }
            parsedDate = DateTime.SpecifyKind(date.Date, DateTimeKind.Utc);
        }
        
        var result = await _mediator.Send(new SearchTripsQuery(source, destination, parsedDate));
        return Ok(result);
    }

    /// <summary>
    /// Get trip details by ID
    /// </summary>
    [HttpGet("trips/{tripId}")]
    public async Task<ActionResult> GetTripById(Guid tripId)
    {
        var result = await _mediator.Send(new GetTripByIdQuery(tripId));
        return Ok(result);
    }
}