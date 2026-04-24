using BusBooking.Application.Common.Interfaces;
using BusBooking.Application.Trips.DTOs;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace BusBooking.Application.Trips.Queries.GetBusTrips;

public record GetBusTripsQuery(Guid BusId) : IRequest<List<TripDto>>;

public class GetBusTripsQueryHandler : IRequestHandler<GetBusTripsQuery, List<TripDto>>
{
    private readonly IAppDbContext _context;

    public GetBusTripsQueryHandler(IAppDbContext context)
    {
        _context = context;
    }

    public async Task<List<TripDto>> Handle(GetBusTripsQuery request, CancellationToken cancellationToken)
    {
        var trips = await _context.Trips
            .Include(t => t.Bus)
            .Include(t => t.Route)
            .Include(t => t.Bookings)
            .Where(t => t.BusId == request.BusId)
            .OrderBy(t => t.DepartureDateTime)
            .Select(t => new TripDto
            {
                Id = t.Id,
                TripDate = t.DepartureDateTime.Date,
                SourceAddress = t.SourceAddress ?? t.Route.Source,
                DestinationAddress = t.DestinationAddress ?? t.Route.Destination,
                PickupPoint = t.PickupPoint ?? "Main Pickup Point",
                DropPoint = t.DropPoint ?? "Main Drop Point",
                DepartureTime = t.DepartureDateTime.ToString("HH:mm"),
                ArrivalTime = t.ArrivalDateTime.ToString("HH:mm"),
                BasePrice = _context.TripPricings
                    .Where(tp => tp.TripId == t.Id)
                    .Select(tp => tp.Price)
                    .FirstOrDefault(),
                Status = t.Status.ToString(),
                BookedSeats = t.Bookings.Count(b => b.Status == Domain.Enums.BookingStatus.Confirmed),
                AvailableSeats = t.Bus.TotalSeats - t.Bookings.Count(b => b.Status == Domain.Enums.BookingStatus.Confirmed)
            })
            .ToListAsync(cancellationToken);

        return trips;
    }
}
