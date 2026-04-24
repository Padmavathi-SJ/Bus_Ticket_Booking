using BusBooking.Application.Bookings.DTOs;
using BusBooking.Application.Common.Interfaces;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace BusBooking.Application.Bookings.Queries.GetUserBookings;

public record GetUserBookingsQuery(Guid CustomerId) : IRequest<List<BookingDto>>;

public class GetUserBookingsQueryHandler : IRequestHandler<GetUserBookingsQuery, List<BookingDto>>
{
    private readonly IAppDbContext _context;

    public GetUserBookingsQueryHandler(IAppDbContext context)
    {
        _context = context;
    }

    public async Task<List<BookingDto>> Handle(GetUserBookingsQuery request, CancellationToken cancellationToken)
    {
        Console.WriteLine($"[GetUserBookingsQuery] Fetching bookings for customer: {request.CustomerId}");
        
        var bookings = await _context.Bookings
            .Include(b => b.Trip)
                .ThenInclude(t => t.Bus)
                    .ThenInclude(bus => bus.Route)
            .Include(b => b.Trip)
                .ThenInclude(t => t.Bus)
                    .ThenInclude(bus => bus.Operator)
            .Include(b => b.Seats)
                .ThenInclude(s => s.SeatLayout)
            .Include(b => b.Customer)
            .Where(b => b.CustomerId == request.CustomerId)
            .OrderByDescending(b => b.BookingDate)
            .Select(b => new BookingDto
            {
                Id = b.Id,
                BusId = b.Trip.BusId,
                BusName = b.Trip.Bus.BusName,
                BusNumber = b.Trip.Bus.BusNumber,
                OperatorName = b.Trip.Bus.Operator != null ? b.Trip.Bus.Operator.CompanyName : "Admin",
                Source = b.Trip.Bus.Route != null ? b.Trip.Bus.Route.Source : "",
                Destination = b.Trip.Bus.Route != null ? b.Trip.Bus.Route.Destination : "",
                SeatNumbers = b.Seats.Select(s => s.SeatLayout.SeatLabel).ToList(),
                PassengerName = b.Seats.FirstOrDefault() != null ? b.Seats.First().PassengerName : "",
                PassengerEmail = b.Customer.Email,
                PassengerPhone = b.Customer.Phone ?? "",
                TotalAmount = b.TotalAmount,
                BookingDate = b.BookingDate,
                JourneyDate = b.Trip.DepartureDateTime,
                Status = b.Status.ToString()
            })
            .ToListAsync(cancellationToken);

        Console.WriteLine($"[GetUserBookingsQuery] Found {bookings.Count} bookings");
        return bookings;
    }
}
