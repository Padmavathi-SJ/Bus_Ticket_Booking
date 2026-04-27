using BusBooking.Application.Common.Interfaces;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace BusBooking.Application.Admin.Queries.GetRevenue;

public record GetRevenueQuery(DateTime? StartDate, DateTime? EndDate) : IRequest<RevenueResponse>;

public record RevenueResponse(
    decimal TotalRevenue,
    List<BusRevenueDto> BusRevenues,
    List<DailyRevenueDto> DailyRevenues
);

public record BusRevenueDto(
    Guid BusId,
    string BusName,
    string BusNumber,
    string BusType,
    string? OperatorName,
    int TotalBookings,
    int ConfirmedBookings,
    int CancelledBookings,
    decimal TotalRevenue,
    decimal ConfirmedRevenue
);

public record DailyRevenueDto(
    DateTime Date,
    decimal Revenue,
    int Bookings
);

public class GetRevenueQueryHandler : IRequestHandler<GetRevenueQuery, RevenueResponse>
{
    private readonly IAppDbContext _context;

    public GetRevenueQueryHandler(IAppDbContext context)
    {
        _context = context;
    }

    public async Task<RevenueResponse> Handle(GetRevenueQuery request, CancellationToken cancellationToken)
    {
        var startDate = request.StartDate ?? DateTime.UtcNow.AddMonths(-1);
        var endDate = request.EndDate ?? DateTime.UtcNow;

        // Get all bookings within date range
        var bookingsQuery = _context.Bookings
            .Include(b => b.Trip)
                .ThenInclude(t => t.Bus)
                    .ThenInclude(bus => bus.Operator)
                        .ThenInclude(o => o!.User)
            .Include(b => b.Payment)
            .Where(b => b.BookingDate >= startDate && b.BookingDate <= endDate);

        var bookings = await bookingsQuery.ToListAsync(cancellationToken);

        // Calculate bus-wise revenue
        var busRevenues = bookings
            .GroupBy(b => new { b.Trip.BusId, b.Trip.Bus })
            .Select(g => new BusRevenueDto(
                BusId: g.Key.BusId,
                BusName: g.Key.Bus.BusName,
                BusNumber: g.Key.Bus.BusNumber,
                BusType: g.Key.Bus.BusType,
                OperatorName: g.Key.Bus.Operator?.User?.FullName,
                TotalBookings: g.Count(),
                ConfirmedBookings: g.Count(b => b.Status == Domain.Enums.BookingStatus.Confirmed),
                CancelledBookings: g.Count(b => b.Status == Domain.Enums.BookingStatus.Cancelled),
                TotalRevenue: g.Sum(b => b.TotalAmount),
                ConfirmedRevenue: g.Where(b => b.Status == Domain.Enums.BookingStatus.Confirmed)
                    .Sum(b => b.TotalAmount)
            ))
            .OrderByDescending(b => b.TotalRevenue)
            .ToList();

        // Calculate daily revenue
        var dailyRevenues = bookings
            .Where(b => b.Status == Domain.Enums.BookingStatus.Confirmed)
            .GroupBy(b => b.BookingDate.Date)
            .Select(g => new DailyRevenueDto(
                Date: g.Key,
                Revenue: g.Sum(b => b.TotalAmount),
                Bookings: g.Count()
            ))
            .OrderBy(d => d.Date)
            .ToList();

        var totalRevenue = bookings
            .Where(b => b.Status == Domain.Enums.BookingStatus.Confirmed)
            .Sum(b => b.TotalAmount);

        return new RevenueResponse(
            TotalRevenue: totalRevenue,
            BusRevenues: busRevenues,
            DailyRevenues: dailyRevenues
        );
    }
}
