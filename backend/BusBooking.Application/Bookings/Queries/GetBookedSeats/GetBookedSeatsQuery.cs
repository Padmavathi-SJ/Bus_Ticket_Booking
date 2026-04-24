using BusBooking.Application.Common.Interfaces;
using BusBooking.Domain.Enums;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace BusBooking.Application.Bookings.Queries.GetBookedSeats;

public record GetBookedSeatsQuery(Guid BusId) : IRequest<List<string>>;

public class GetBookedSeatsQueryHandler : IRequestHandler<GetBookedSeatsQuery, List<string>>
{
    private readonly IAppDbContext _context;

    public GetBookedSeatsQueryHandler(IAppDbContext context)
    {
        _context = context;
    }

    public async Task<List<string>> Handle(GetBookedSeatsQuery request, CancellationToken cancellationToken)
    {
        // Get all confirmed bookings for trips on this bus (for today or future)
        var today = DateTime.UtcNow.Date;
        
        var bookedSeats = await _context.Bookings
            .Include(b => b.Trip)
            .Include(b => b.Seats)
                .ThenInclude(s => s.SeatLayout)
            .Where(b => b.Trip.BusId == request.BusId
                && b.Trip.DepartureDateTime.Date >= today
                && b.Status == BookingStatus.Confirmed)
            .SelectMany(b => b.Seats.Select(s => s.SeatLayout.SeatLabel))
            .Distinct()
            .ToListAsync(cancellationToken);

        return bookedSeats;
    }
}
