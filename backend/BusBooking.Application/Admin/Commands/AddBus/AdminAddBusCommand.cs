using BusBooking.Application.Common.Interfaces;
using BusBooking.Domain.Entities;
using BusBooking.Domain.Enums;
using MediatR;
using System.ComponentModel.DataAnnotations;

namespace BusBooking.Application.Admin.Commands.AddBus;

public class AdminAddBusCommand : IRequest<Guid>
{
    [Required] public Guid RouteId { get; set; }
    [Required] public string BusNumber { get; set; } = string.Empty;
    [Required] public string BusName { get; set; } = string.Empty;
    [Required] public string BusType { get; set; } = string.Empty;
    [Required] public int TotalSeats { get; set; }
    [Required] public int FemaleSeats { get; set; }
    [Required] public int MaleSeats { get; set; }
    [Required] public double BasePrice { get; set; }
    public string Amenities { get; set; } = string.Empty;
    public Guid? OperatorId { get; set; }
}

public class AdminAddBusCommandHandler : IRequestHandler<AdminAddBusCommand, Guid>
{
    private readonly IAppDbContext _context;

    public AdminAddBusCommandHandler(IAppDbContext context)
    {
        _context = context;
    }

    public async Task<Guid> Handle(AdminAddBusCommand request, CancellationToken cancellationToken)
    {
        var bus = new Bus
        {
            Id = Guid.NewGuid(),
            RouteId = request.RouteId,
            BusNumber = request.BusNumber,
            BusName = request.BusName,
            BusType = request.BusType,
            TotalSeats = request.TotalSeats,
            FemaleSeats = request.FemaleSeats,
            MaleSeats = request.MaleSeats,
            BasePrice = request.BasePrice,
            Amenities = request.Amenities,
            OperatorId = request.OperatorId,
            Status = BusStatus.Approved, // Admins don't need approval for their own buses
            IsAvailable = true,
            CreatedAt = DateTime.UtcNow
        };

        _context.Buses.Add(bus);
        await _context.SaveChangesAsync(cancellationToken);

        return bus.Id;
    }
}
