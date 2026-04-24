using BusBooking.Application.Common.Interfaces;
using BusBooking.Domain.Entities;
using MediatR;
using System.ComponentModel.DataAnnotations;

namespace BusBooking.Application.Stations.Commands.CreateStation;

public class CreateStationCommand : IRequest<Guid>
{
    [Required] public string Name { get; set; } = string.Empty;
    [Required] public string City { get; set; } = string.Empty;
    [Required] public string State { get; set; } = string.Empty;
    [Required] public string Code { get; set; } = string.Empty;
}

public class CreateStationCommandHandler : IRequestHandler<CreateStationCommand, Guid>
{
    private readonly IAppDbContext _context;

    public CreateStationCommandHandler(IAppDbContext context)
    {
        _context = context;
    }

    public async Task<Guid> Handle(CreateStationCommand request, CancellationToken cancellationToken)
    {
        var station = new Station
        {
            Id = Guid.NewGuid(),
            Name = request.Name,
            City = request.City,
            State = request.State,
            Code = request.Code,
            IsActive = true,
            CreatedAt = DateTime.UtcNow
        };

        _context.Stations.Add(station);
        await _context.SaveChangesAsync(cancellationToken);

        return station.Id;
    }
}
