using BusBooking.Application.Common.Interfaces;
using BusBooking.Application.Stations.DTOs;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace BusBooking.Application.Stations.Queries.GetStations;

public record GetStationsQuery : IRequest<List<StationDto>>;

public class GetStationsQueryHandler : IRequestHandler<GetStationsQuery, List<StationDto>>
{
    private readonly IAppDbContext _context;

    public GetStationsQueryHandler(IAppDbContext context)
    {
        _context = context;
    }

    public async Task<List<StationDto>> Handle(GetStationsQuery request, CancellationToken cancellationToken)
    {
        return await _context.Stations
            .Where(s => s.IsActive)
            .Select(s => new StationDto
            {
                Id = s.Id,
                Name = s.Name,
                City = s.City,
                State = s.State,
                Code = s.Code,
                IsActive = s.IsActive
            })
            .OrderBy(s => s.Name)
            .ToListAsync(cancellationToken);
    }
}
