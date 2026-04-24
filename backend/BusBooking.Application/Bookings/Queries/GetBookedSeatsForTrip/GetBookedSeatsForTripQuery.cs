using BusBooking.Application.Common.Interfaces;
using BusBooking.Domain.Enums;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace BusBooking.Application.Bookings.Queries.GetBookedSeatsForTrip;

public record GetBookedSeatsForTripQuery(Guid TripId) : IRequest<List<string>>;

public class GetBookedSeatsForTripQueryHandler : IRequestHandler<GetBookedSeatsForTripQuery, List<string>>
{
    private readonly IAppDbContext _context;

    public GetBookedSeatsForTripQueryHandler(IAppDbContext context)
    {
        _context = context;
    }

    public async Task<List<string>> Handle(GetBookedSeatsForTripQuery request, CancellationToken cancellationToken)
    {
        var bookedSeats = await _context.Bookings
            .Where(b => b.TripId == request.TripId && b.Status == BookingStatus.Confirmed)
            .SelectMany(b => b.Seats.Select(s => s.SeatLayout.SeatLabel))
            .Distinct()
            .ToListAsync(cancellationToken);

        return bookedSeats;
    }
}
