using BusBooking.Application.Bookings.DTOs;
using BusBooking.Application.Common.Interfaces;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace BusBooking.Application.Bookings.Queries.GetOperatorBookings;

public record GetOperatorBookingsQuery(Guid OperatorId) : IRequest<List<OperatorBookingDto>>;

public class GetOperatorBookingsQueryHandler : IRequestHandler<GetOperatorBookingsQuery, List<OperatorBookingDto>>
{
    private readonly IAppDbContext _context;

    public GetOperatorBookingsQueryHandler(IAppDbContext context)
    {
        _context = context;
    }

    public async Task<List<OperatorBookingDto>> Handle(GetOperatorBookingsQuery request, CancellationToken cancellationToken)
    {
        var bookings = await _context.Bookings
            .Include(b => b.Trip)
                .ThenInclude(t => t.Bus)
                    .ThenInclude(bus => bus.Route)
            .Include(b => b.Seats)
                .ThenInclude(s => s.SeatLayout)
            .Include(b => b.Customer)
            .Where(b => b.Trip.Bus.OperatorId == request.OperatorId)
            .OrderByDescending(b => b.BookingDate)
            .Select(b => new OperatorBookingDto
            {
                Id = b.Id,
                BookingReference = b.Id.ToString().Substring(0, 8).ToUpper(),
                BusId = b.Trip.BusId,
                BusName = b.Trip.Bus.BusName,
                BusNumber = b.Trip.Bus.BusNumber,
                BusType = b.Trip.Bus.BusType,
                Source = b.Trip.Bus.Route != null ? b.Trip.Bus.Route.Source : "",
                Destination = b.Trip.Bus.Route != null ? b.Trip.Bus.Route.Destination : "",
                SeatNumbers = b.Seats.Select(s => s.SeatLayout.SeatLabel).ToList(),
                PassengerName = b.Seats.FirstOrDefault() != null ? b.Seats.First().PassengerName : "",
                PassengerEmail = b.Customer.Email,
                PassengerPhone = b.Customer.Phone ?? "",
                PassengerAge = b.Seats.FirstOrDefault() != null ? b.Seats.First().Age : 0,
                PassengerGender = b.Seats.FirstOrDefault() != null ? b.Seats.First().Gender : "",
                TotalAmount = b.TotalAmount,
                BookingDate = b.BookingDate,
                JourneyDate = b.Trip.DepartureDateTime,
                Status = b.Status.ToString()
            })
            .ToListAsync(cancellationToken);

        return bookings;
    }
}
