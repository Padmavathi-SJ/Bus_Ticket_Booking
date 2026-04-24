using BusBooking.Application.Common.Interfaces;
using BusBooking.Application.Routes.DTOs;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace BusBooking.Application.Routes.Queries.GetRoutes;

public record GetRoutesQuery : IRequest<List<RouteDto>>;

public class GetRoutesQueryHandler : IRequestHandler<GetRoutesQuery, List<RouteDto>>
{
    private readonly IAppDbContext _context;

    public GetRoutesQueryHandler(IAppDbContext context)
    {
        _context = context;
    }

    public async Task<List<RouteDto>> Handle(GetRoutesQuery request, CancellationToken cancellationToken)
    {
        return await _context.Routes
            .Where(r => r.IsActive)
            .Select(r => new RouteDto
            {
                Id = r.Id,
                Name = r.Name,
                Source = r.Source,
                Destination = r.Destination,
                DistanceKm = r.DistanceKm,
                IsActive = r.IsActive
            })
            .ToListAsync(cancellationToken);
    }
}
