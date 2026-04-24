using BusBooking.Application.Buses.DTOs;
using BusBooking.Application.Common.Interfaces;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace BusBooking.Application.Admin.Queries.GetBuses;

public record GetBusesQuery(string? Source = null, string? Destination = null) : IRequest<List<BusDto>>;

public class GetBusesQueryHandler : IRequestHandler<GetBusesQuery, List<BusDto>>
{
    private readonly IAppDbContext _context;

    public GetBusesQueryHandler(IAppDbContext context)
    {
        _context = context;
    }

    public async Task<List<BusDto>> Handle(GetBusesQuery request, CancellationToken cancellationToken)
    {
        var source = request.Source?.Trim();
        var destination = request.Destination?.Trim();

        var query = _context.Buses
            .Include(b => b.Operator)
            .Include(b => b.Route)
            .AsQueryable();

        if (!string.IsNullOrEmpty(source))
        {
            query = query.Where(b =>
                b.Route != null &&
                b.Route.Source != null &&
                b.Route.Source.ToLower().Contains(source.ToLower()));
        }

        if (!string.IsNullOrEmpty(destination))
        {
            query = query.Where(b =>
                b.Route != null &&
                b.Route.Destination != null &&
                b.Route.Destination.ToLower().Contains(destination.ToLower()));
        }

        return await query
            .Select(b => new BusDto
            {
                Id = b.Id,
                BusName = b.BusName,
                BusNumber = b.BusNumber,
                BusType = b.BusType,
                TotalSeats = b.TotalSeats,
                BasePrice = b.BasePrice,
                Amenities = b.Amenities,
                Status = b.Status,
                IsAvailable = b.IsAvailable,
                RouteId = b.RouteId,
                RouteName = b.Route != null ? b.Route.Name : null,
                Source = b.Route != null ? b.Route.Source : null,
                Destination = b.Route != null ? b.Route.Destination : null,
                OperatorId = b.OperatorId,
                OperatorName = b.Operator != null ? b.Operator.CompanyName : "Admin",
                FemaleSeats = b.FemaleSeats,
                MaleSeats = b.MaleSeats
            })
            .ToListAsync(cancellationToken);
    }
}
