using BusBooking.Application.Buses.DTOs;
using BusBooking.Application.Common.Interfaces;
using BusBooking.Domain.Enums;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace BusBooking.Application.Admin.Queries.GetPendingBuses;

public record GetPendingBusesQuery : IRequest<List<BusDto>>;

public class GetPendingBusesQueryHandler : IRequestHandler<GetPendingBusesQuery, List<BusDto>>
{
    private readonly IAppDbContext _context;

    public GetPendingBusesQueryHandler(IAppDbContext context)
    {
        _context = context;
    }

    public async Task<List<BusDto>> Handle(GetPendingBusesQuery request, CancellationToken cancellationToken)
    {
        return await _context.Buses
            .Include(b => b.Operator)
            .Include(b => b.Route)
            .Where(b => b.Status == BusStatus.Pending)
            .Select(b => new BusDto
            {
                Id = b.Id,
                OperatorId = b.OperatorId,
                OperatorName = b.Operator.CompanyName,
                RouteId = b.RouteId,
                RouteName = b.Route.Name,
                BusNumber = b.BusNumber,
                BusName = b.BusName,
                BusType = b.BusType,
                TotalSeats = b.TotalSeats,
                FemaleSeats = b.FemaleSeats,
                MaleSeats = b.MaleSeats,
                Status = b.Status
            })
            .ToListAsync(cancellationToken);
    }
}
