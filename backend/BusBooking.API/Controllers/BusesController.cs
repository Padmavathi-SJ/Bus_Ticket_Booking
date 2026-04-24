using BusBooking.Application.Admin.Queries.GetBuses;
using BusBooking.Application.Buses.Queries.GetBusById;
using BusBooking.Application.Trips.Queries.SearchTrips;
using BusBooking.Application.Trips.Queries.GetTripById;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Linq;

namespace BusBooking.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class BusesController : ControllerBase
{
    private readonly IMediator _mediator;

    public BusesController(IMediator mediator)
    {
        _mediator = mediator;
    }

    [HttpGet("{id}")]
    public async Task<ActionResult> GetBusById(Guid id)
    {
        var result = await _mediator.Send(new GetBusByIdQuery(id));
        
        if (result == null)
            return NotFound(new { message = "Bus not found or not available" });
        
        return Ok(result);
    }

    [HttpGet("search")]
    public async Task<ActionResult> SearchBuses([FromQuery] string? source, [FromQuery] string? destination)
    {
        // Public search - filtered to only show approved and active buses
        var result = await _mediator.Send(new GetBusesQuery(source, destination));
        Console.WriteLine($"[DEBUG-PUBLIC] Total buses fetched: {result.Count()}");
        
        // Return only approved and available buses to public
        var availableBuses = result.Where(b => 
            b.Status == BusBooking.Domain.Enums.BusStatus.Approved && 
            b.IsAvailable);
            
        Console.WriteLine($"[DEBUG-PUBLIC] Approved & Available buses: {availableBuses.Count()}");

        // Flexible filter: If both are empty, show all. If one is provided, filter by that.
        if (!string.IsNullOrEmpty(source) || !string.IsNullOrEmpty(destination))
        {
            availableBuses = availableBuses.Where(b => 
                (string.IsNullOrEmpty(source) || (!string.IsNullOrEmpty(b.Source) && b.Source.Contains(source, StringComparison.OrdinalIgnoreCase))) &&
                (string.IsNullOrEmpty(destination) || (!string.IsNullOrEmpty(b.Destination) && b.Destination.Contains(destination, StringComparison.OrdinalIgnoreCase))));
            Console.WriteLine($"[DEBUG-PUBLIC] After filtering for {source} -> {destination}: {availableBuses.Count()}");
        }
            
        return Ok(availableBuses.ToList());
    }

    [HttpGet("search-trips")]
    public async Task<ActionResult> SearchTrips(
        [FromQuery] string? source, 
        [FromQuery] string? destination,
        [FromQuery] string? tripDate)
    {
        try
        {
            DateTime? parsedDate = null;
            
            if (!string.IsNullOrEmpty(tripDate))
            {
                // Parse the date string and treat it as UTC
                if (DateTime.TryParse(tripDate, out var date))
                {
                    parsedDate = DateTime.SpecifyKind(date.Date, DateTimeKind.Utc);
                    Console.WriteLine($"[DEBUG-TRIPS] Parsed date: {parsedDate:yyyy-MM-dd HH:mm:ss} (Kind: {parsedDate.Value.Kind})");
                }
                else
                {
                    return BadRequest(new { message = "Invalid date format. Use YYYY-MM-DD" });
                }
            }
            
            Console.WriteLine($"[DEBUG-TRIPS] Searching trips: Source={source}, Destination={destination}, Date={parsedDate}");
            
            var result = await _mediator.Send(new SearchTripsQuery(source, destination, parsedDate));
            
            Console.WriteLine($"[DEBUG-TRIPS] Found {result.Count} trips");
            
            return Ok(result);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[ERROR-TRIPS] Search failed: {ex.Message}");
            Console.WriteLine($"[ERROR-TRIPS] Stack trace: {ex.StackTrace}");
            return BadRequest(new { message = ex.Message });
        }
    }

    [HttpGet("trips/{tripId}")]
    public async Task<ActionResult> GetTripById(Guid tripId)
    {
        try
        {
            Console.WriteLine($"[DEBUG-TRIP] Getting trip details for: {tripId}");
            
            var result = await _mediator.Send(new GetTripByIdQuery(tripId));
            
            Console.WriteLine($"[DEBUG-TRIP] Trip found: {result.BusName}");
            
            return Ok(result);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[ERROR-TRIP] Get trip failed: {ex.Message}");
            return NotFound(new { message = ex.Message });
        }
    }
}
