using BusBooking.Application.Buses.DTOs;
using BusBooking.Application.Common.Interfaces;
using BusBooking.Domain.Enums;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace BusBooking.Application.Buses.Queries.GetBusById;

public record GetBusByIdQuery(Guid BusId) : IRequest<BusDto?>;

public class GetBusByIdQueryHandler : IRequestHandler<GetBusByIdQuery, BusDto?>
{
    private readonly IAppDbContext _context;

    public GetBusByIdQueryHandler(IAppDbContext context)
    {
        _context = context;
    }

    public async Task<BusDto?> Handle(GetBusByIdQuery request, CancellationToken cancellationToken)
    {
        var bus = await _context.Buses
            .Include(b => b.Route)
            .Include(b => b.Operator)
            .Where(b => b.Id == request.BusId 
                && b.Status == BusStatus.Approved 
                && b.IsAvailable)
            .Select(b => new BusDto
            {
                Id = b.Id,
                BusName = b.BusName,
                BusNumber = b.BusNumber,
                BusType = b.BusType,
                OperatorName = b.Operator != null ? b.Operator.CompanyName : "Admin",
                Source = b.Route != null ? b.Route.Source : null,
                Destination = b.Route != null ? b.Route.Destination : null,
                BasePrice = b.BasePrice,
                TotalSeats = b.TotalSeats,
                FemaleSeats = b.FemaleSeats,
                MaleSeats = b.MaleSeats,
                Amenities = b.Amenities,
                Status = b.Status,
                IsAvailable = b.IsAvailable,
                RouteId = b.RouteId,
                RouteName = b.Route != null ? b.Route.Name : null,
                OperatorId = b.OperatorId
            })
            .FirstOrDefaultAsync(cancellationToken);

        return bus;
    }
}
