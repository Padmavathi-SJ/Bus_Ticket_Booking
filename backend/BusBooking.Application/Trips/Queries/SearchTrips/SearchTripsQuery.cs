using BusBooking.Application.Common.Interfaces;
using BusBooking.Application.Trips.DTOs;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace BusBooking.Application.Trips.Queries.SearchTrips;

public record SearchTripsQuery(string? Source, string? Destination, DateTime? TripDate) : IRequest<List<TripSearchDto>>;

public class SearchTripsQueryHandler : IRequestHandler<SearchTripsQuery, List<TripSearchDto>>
{
    private readonly IAppDbContext _context;

    public SearchTripsQueryHandler(IAppDbContext context)
    {
        _context = context;
    }

    public async Task<List<TripSearchDto>> Handle(SearchTripsQuery request, CancellationToken cancellationToken)
    {
        Console.WriteLine($"[DEBUG-SEARCH] ========== SEARCH TRIPS QUERY ==========");
        Console.WriteLine($"[DEBUG-SEARCH] Filters - Source: {request.Source ?? "ANY"}, Destination: {request.Destination ?? "ANY"}, Date: {request.TripDate?.ToString("yyyy-MM-dd") ?? "TODAY+"}");
        
        var query = _context.Trips
            .Include(t => t.Bus)
                .ThenInclude(b => b.Operator)
            .Include(t => t.Route)
            .Include(t => t.Bookings)
            .Include(t => t.Pricing)
            .Where(t => t.Status == Domain.Enums.TripStatus.Scheduled)
            .Where(t => t.Bus.Status == Domain.Enums.BusStatus.Approved)
            .Where(t => t.Bus.IsAvailable == true) // Only show available buses
            .AsQueryable();

        Console.WriteLine($"[DEBUG-SEARCH] Initial query filters applied: TripStatus=Scheduled, BusStatus=Approved, IsAvailable=true");

        // Filter by date if provided
        if (request.TripDate.HasValue)
        {
            // Ensure the date is treated as UTC to avoid timezone issues
            var searchDate = DateTime.SpecifyKind(request.TripDate.Value.Date, DateTimeKind.Utc);
            var nextDay = searchDate.AddDays(1);
            
            Console.WriteLine($"[DEBUG-SEARCH] Searching for trips on date: {searchDate:yyyy-MM-dd}");
            
            // Use range comparison to avoid Date property issues with PostgreSQL
            query = query.Where(t => t.DepartureDateTime >= searchDate && t.DepartureDateTime < nextDay);
        }
        else
        {
            // Default to today and future trips
            var today = DateTime.SpecifyKind(DateTime.UtcNow.Date, DateTimeKind.Utc);
            query = query.Where(t => t.DepartureDateTime >= today);
        }

        // Filter by source if provided
        if (!string.IsNullOrWhiteSpace(request.Source))
        {
            var sourceLower = request.Source.ToLower();
            query = query.Where(t => 
                (t.SourceAddress != null && t.SourceAddress.ToLower().Contains(sourceLower)) ||
                t.Route.Source.ToLower().Contains(sourceLower));
        }

        // Filter by destination if provided
        if (!string.IsNullOrWhiteSpace(request.Destination))
        {
            var destLower = request.Destination.ToLower();
            query = query.Where(t => 
                (t.DestinationAddress != null && t.DestinationAddress.ToLower().Contains(destLower)) ||
                t.Route.Destination.ToLower().Contains(destLower));
        }

        var trips = await query
            .OrderBy(t => t.DepartureDateTime)
            .Select(t => new TripSearchDto
            {
                TripId = t.Id,
                BusId = t.BusId,
                BusName = t.Bus.BusName,
                BusNumber = t.Bus.BusNumber,
                BusType = t.Bus.BusType,
                OperatorName = t.Bus.Operator != null ? t.Bus.Operator.CompanyName : "Unknown",
                TotalSeats = t.Bus.TotalSeats,
                FemaleSeats = t.Bus.FemaleSeats,
                MaleSeats = t.Bus.MaleSeats,
                Amenities = t.Bus.Amenities,
                
                // Trip specific details
                TripDate = t.DepartureDateTime.Date,
                SourceAddress = t.SourceAddress ?? t.Route.Source,
                DestinationAddress = t.DestinationAddress ?? t.Route.Destination,
                PickupPoint = t.PickupPoint ?? "Main Pickup Point",
                DropPoint = t.DropPoint ?? "Main Drop Point",
                DepartureTime = t.DepartureDateTime.ToString("HH:mm"),
                ArrivalTime = t.ArrivalDateTime.ToString("HH:mm"),
                Duration = (int)(t.ArrivalDateTime - t.DepartureDateTime).TotalMinutes,
                
                // Pricing
                BasePrice = t.Pricing.Any() ? t.Pricing.First().Price : 0,
                
                // Availability
                BookedSeats = t.Bookings.Count(b => b.Status == Domain.Enums.BookingStatus.Confirmed),
                AvailableSeats = t.Bus.TotalSeats - t.Bookings.Count(b => b.Status == Domain.Enums.BookingStatus.Confirmed),
                IsAvailable = t.Bus.IsAvailable
            })
            .ToListAsync(cancellationToken);

        Console.WriteLine($"[DEBUG-SEARCH] ========== RESULTS ==========");
        Console.WriteLine($"[DEBUG-SEARCH] Query returned {trips.Count} trips");
        
        if (trips.Count == 0)
        {
            Console.WriteLine($"[DEBUG-SEARCH] ⚠️ NO TRIPS FOUND!");
            Console.WriteLine($"[DEBUG-SEARCH] Possible reasons:");
            Console.WriteLine($"[DEBUG-SEARCH]   1. No trips scheduled for available buses");
            Console.WriteLine($"[DEBUG-SEARCH]   2. All trips are in the past");
            Console.WriteLine($"[DEBUG-SEARCH]   3. No buses match the search criteria");
            Console.WriteLine($"[DEBUG-SEARCH]   4. Buses exist but have IsAvailable=false");
        }
        else
        {
            Console.WriteLine($"[DEBUG-SEARCH] Found {trips.Count} trip(s):");
            foreach (var trip in trips.Take(10))
            {
                Console.WriteLine($"[DEBUG-SEARCH]   - {trip.BusName} ({trip.BusNumber}) | {trip.SourceAddress} → {trip.DestinationAddress} | {trip.TripDate:yyyy-MM-dd} {trip.DepartureTime} | IsAvailable={trip.IsAvailable}");
            }
        }
        Console.WriteLine($"[DEBUG-SEARCH] =====================================");

        return trips;
    }
}
