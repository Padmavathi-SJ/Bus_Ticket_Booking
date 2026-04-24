using BusBooking.Application.Common.Interfaces;
using BusBooking.Domain.Enums;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace BusBooking.Application.Operator.Queries.GetOperatorDashboardStats;

public record GetOperatorDashboardStatsQuery(Guid OperatorId) : IRequest<OperatorDashboardStatsDto>;

public class OperatorDashboardStatsDto
{
    public int TotalBuses { get; set; }
    public int ActiveBuses { get; set; }
    public int PendingBuses { get; set; }
    public int TotalTrips { get; set; }
    public int UpcomingTrips { get; set; }
    public int TotalBookings { get; set; }
    public int TodayBookings { get; set; }
    public int TotalSeatsBooked { get; set; }
    public decimal TotalRevenue { get; set; }
    public decimal TodayRevenue { get; set; }
}

public class GetOperatorDashboardStatsQueryHandler : IRequestHandler<GetOperatorDashboardStatsQuery, OperatorDashboardStatsDto>
{
    private readonly IAppDbContext _context;

    public GetOperatorDashboardStatsQueryHandler(IAppDbContext context)
    {
        _context = context;
    }

    public async Task<OperatorDashboardStatsDto> Handle(GetOperatorDashboardStatsQuery request, CancellationToken cancellationToken)
    {
        var today = DateTime.UtcNow.Date;
        var tomorrow = today.AddDays(1);
        var now = DateTime.UtcNow;

        // Get buses for this operator
        var totalBuses = await _context.Buses
            .CountAsync(b => b.OperatorId == request.OperatorId, cancellationToken);

        var activeBuses = await _context.Buses
            .CountAsync(b => b.OperatorId == request.OperatorId 
                            && b.Status == BusStatus.Approved 
                            && b.IsAvailable, cancellationToken);

        var pendingBuses = await _context.Buses
            .CountAsync(b => b.OperatorId == request.OperatorId 
                            && b.Status == BusStatus.Pending, cancellationToken);

        // Get trips for this operator's buses
        var busIds = await _context.Buses
            .Where(b => b.OperatorId == request.OperatorId)
            .Select(b => b.Id)
            .ToListAsync(cancellationToken);

        var totalTrips = await _context.Trips
            .CountAsync(t => busIds.Contains(t.BusId), cancellationToken);

        var upcomingTrips = await _context.Trips
            .CountAsync(t => busIds.Contains(t.BusId) 
                            && t.DepartureDateTime >= now, cancellationToken);

        // Get bookings for this operator's buses
        var tripIds = await _context.Trips
            .Where(t => busIds.Contains(t.BusId))
            .Select(t => t.Id)
            .ToListAsync(cancellationToken);

        var totalBookings = await _context.Bookings
            .CountAsync(b => tripIds.Contains(b.TripId), cancellationToken);

        var todayBookings = await _context.Bookings
            .CountAsync(b => tripIds.Contains(b.TripId) 
                            && b.CreatedAt >= today 
                            && b.CreatedAt < tomorrow, cancellationToken);

        var totalSeatsBooked = await _context.BookingSeats
            .Where(bs => tripIds.Contains(bs.Booking.TripId))
            .CountAsync(cancellationToken);

        var totalRevenue = await _context.Bookings
            .Where(b => tripIds.Contains(b.TripId) 
                       && b.Status == BookingStatus.Confirmed)
            .SumAsync(b => b.TotalAmount, cancellationToken);

        var todayRevenue = await _context.Bookings
            .Where(b => tripIds.Contains(b.TripId) 
                       && b.Status == BookingStatus.Confirmed
                       && b.CreatedAt >= today 
                       && b.CreatedAt < tomorrow)
            .SumAsync(b => b.TotalAmount, cancellationToken);

        return new OperatorDashboardStatsDto
        {
            TotalBuses = totalBuses,
            ActiveBuses = activeBuses,
            PendingBuses = pendingBuses,
            TotalTrips = totalTrips,
            UpcomingTrips = upcomingTrips,
            TotalBookings = totalBookings,
            TodayBookings = todayBookings,
            TotalSeatsBooked = totalSeatsBooked,
            TotalRevenue = totalRevenue,
            TodayRevenue = todayRevenue
        };
    }
}
