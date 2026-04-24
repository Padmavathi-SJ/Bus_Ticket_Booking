using BusBooking.Application.Common.Interfaces;
using BusBooking.Domain.Enums;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace BusBooking.Application.Admin.Queries.GetDashboardStats;

public record GetDashboardStatsQuery : IRequest<DashboardStatsDto>;

public class DashboardStatsDto
{
    public int TotalOperators { get; set; }
    public int PendingOperators { get; set; }
    public int TotalBuses { get; set; }
    public int PendingBuses { get; set; }
    public int ActiveBuses { get; set; }
    public int TotalRoutes { get; set; }
    public int TotalBookings { get; set; }
    public int TodayBookings { get; set; }
    public decimal TotalRevenue { get; set; }
    public decimal TodayRevenue { get; set; }
}

public class GetDashboardStatsQueryHandler : IRequestHandler<GetDashboardStatsQuery, DashboardStatsDto>
{
    private readonly IAppDbContext _context;

    public GetDashboardStatsQueryHandler(IAppDbContext context)
    {
        _context = context;
    }

    public async Task<DashboardStatsDto> Handle(GetDashboardStatsQuery request, CancellationToken cancellationToken)
    {
        var today = DateTime.UtcNow.Date;
        var tomorrow = today.AddDays(1);

        var totalOperators = await _context.BusOperators.CountAsync(cancellationToken);
        var pendingOperators = await _context.BusOperators
            .CountAsync(o => o.Status == OperatorStatus.Pending, cancellationToken);

        var totalBuses = await _context.Buses.CountAsync(cancellationToken);
        var pendingBuses = await _context.Buses
            .CountAsync(b => b.Status == BusStatus.Pending, cancellationToken);
        var activeBuses = await _context.Buses
            .CountAsync(b => b.Status == BusStatus.Approved && b.IsAvailable, cancellationToken);

        var totalRoutes = await _context.Routes.CountAsync(r => r.IsActive, cancellationToken);

        var totalBookings = await _context.Bookings.CountAsync(cancellationToken);
        var todayBookings = await _context.Bookings
            .CountAsync(b => b.CreatedAt >= today && b.CreatedAt < tomorrow, cancellationToken);

        var totalRevenue = await _context.Bookings
            .Where(b => b.Status == BookingStatus.Confirmed)
            .SumAsync(b => b.TotalAmount, cancellationToken);

        var todayRevenue = await _context.Bookings
            .Where(b => b.Status == BookingStatus.Confirmed 
                        && b.CreatedAt >= today && b.CreatedAt < tomorrow)
            .SumAsync(b => b.TotalAmount, cancellationToken);

        return new DashboardStatsDto
        {
            TotalOperators = totalOperators,
            PendingOperators = pendingOperators,
            TotalBuses = totalBuses,
            PendingBuses = pendingBuses,
            ActiveBuses = activeBuses,
            TotalRoutes = totalRoutes,
            TotalBookings = totalBookings,
            TodayBookings = todayBookings,
            TotalRevenue = totalRevenue,
            TodayRevenue = todayRevenue
        };
    }
}
