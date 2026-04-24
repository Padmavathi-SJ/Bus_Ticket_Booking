using BusBooking.Application.Common.Interfaces;
using MediatR;

namespace BusBooking.Application.Trips.Commands.ScheduleTrip;

public record ScheduleTripCommand : IRequest<Guid>
{
    public Guid BusId { get; init; }
    public Guid RouteId { get; init; }
    public string SourceAddress { get; init; } = string.Empty;
    public string DestinationAddress { get; init; } = string.Empty;
    public string PickupPoint { get; init; } = string.Empty;
    public string DropPoint { get; init; } = string.Empty;
    public DateTime DepartureDateTime { get; init; }
    public DateTime ArrivalDateTime { get; init; }
    public decimal BasePrice { get; init; }
}

public class ScheduleTripCommandHandler : IRequestHandler<ScheduleTripCommand, Guid>
{
    private readonly IAppDbContext _context;

    public ScheduleTripCommandHandler(IAppDbContext context)
    {
        _context = context;
    }

    public async Task<Guid> Handle(ScheduleTripCommand request, CancellationToken cancellationToken)
    {
        // Verify bus exists and belongs to the operator
        var bus = await _context.Buses
            .FindAsync(new object[] { request.BusId }, cancellationToken);

        if (bus == null)
        {
            throw new Exception("Bus not found");
        }

        // Verify route exists
        var route = await _context.Routes
            .FindAsync(new object[] { request.RouteId }, cancellationToken);

        if (route == null)
        {
            throw new Exception("Route not found");
        }

        // Ensure dates are stored as UTC
        var departureUtc = DateTime.SpecifyKind(request.DepartureDateTime, DateTimeKind.Utc);
        var arrivalUtc = DateTime.SpecifyKind(request.ArrivalDateTime, DateTimeKind.Utc);

        Console.WriteLine($"[DEBUG-SCHEDULE] Creating trip for {bus.BusName} on {departureUtc:yyyy-MM-dd HH:mm:ss} UTC");

        // Create the trip
        var trip = new Domain.Entities.Trip
        {
            Id = Guid.NewGuid(),
            BusId = request.BusId,
            RouteId = request.RouteId,
            SourceAddress = request.SourceAddress,
            DestinationAddress = request.DestinationAddress,
            PickupPoint = request.PickupPoint,
            DropPoint = request.DropPoint,
            DepartureDateTime = departureUtc,
            ArrivalDateTime = arrivalUtc,
            Status = Domain.Enums.TripStatus.Scheduled,
            CreatedAt = DateTime.UtcNow
        };

        _context.Trips.Add(trip);

        // Create a single pricing entry for all seat types
        var pricing = new Domain.Entities.TripPricing
        {
            Id = Guid.NewGuid(),
            TripId = trip.Id,
            SeatType = Domain.Enums.SeatType.Seater, // Default seat type
            Price = request.BasePrice,
            CreatedAt = DateTime.UtcNow
        };
        _context.TripPricings.Add(pricing);

        await _context.SaveChangesAsync(cancellationToken);

        Console.WriteLine($"[DEBUG-SCHEDULE] Trip created successfully with ID: {trip.Id}");

        return trip.Id;
    }
}
