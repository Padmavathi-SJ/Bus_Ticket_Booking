using BusBooking.Application.Common.Interfaces;
using BusBooking.Domain.Entities;
using MediatR;
using System.ComponentModel.DataAnnotations;

namespace BusBooking.Application.Routes.Commands.CreateRoute;

public class CreateRouteCommand : IRequest<Guid>
{
    [Required] public string Name { get; set; } = string.Empty;
    [Required] public string Source { get; set; } = string.Empty;
    [Required] public string Destination { get; set; } = string.Empty;
    public double DistanceKm { get; set; }
}

public class CreateRouteCommandHandler : IRequestHandler<CreateRouteCommand, Guid>
{
    private readonly IAppDbContext _context;

    public CreateRouteCommandHandler(IAppDbContext context)
    {
        _context = context;
    }

    public async Task<Guid> Handle(CreateRouteCommand request, CancellationToken cancellationToken)
    {
        var route = new BusRoute
        {
            Id = Guid.NewGuid(),
            Name = request.Name,
            Source = request.Source,
            Destination = request.Destination,
            DistanceKm = request.DistanceKm,
            IsActive = true,
            CreatedAt = DateTime.UtcNow
        };

        _context.Routes.Add(route);
        await _context.SaveChangesAsync(cancellationToken);

        return route.Id;
    }
}
