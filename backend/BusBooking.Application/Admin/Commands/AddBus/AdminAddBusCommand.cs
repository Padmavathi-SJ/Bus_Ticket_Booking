
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

// Application/Commands/Admin/AdminAddBusCommand.cs
using BusBooking.Application.Commands;

namespace BusBooking.Application.Admin.Commands.AddBus;

public class AdminAddBusCommand : BaseBusCommand
{
    public Guid? OperatorId { get; set; }  // Admin can specify operator
    
    protected override BusStatus GetInitialStatus() 
        => BusStatus.Approved;  // Admin's buses are auto-approved
    
    protected override bool GetInitialAvailability() 
        => true;  // Admin's buses are immediately available
    
    protected override Guid? GetOperatorId() 
        => OperatorId;  // Admin can assign to operator or null for system bus
}

// Handler becomes VERY simple!
public class AdminAddBusCommandHandler : BaseBusCommandHandler<AdminAddBusCommand>
{
    public AdminAddBusCommandHandler(IAppDbContext context) : base(context) { }
    
    // No need to override anything unless we need special logic
    // The base handler does all the work!
}
