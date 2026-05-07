
using BusBooking.Application.Common.Interfaces;
using BusBooking.Domain.Entities;
using BusBooking.Domain.Enums;
using MediatR;
using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;

namespace BusBooking.Application.Buses.Commands.CreateBus;

public class CreateBusCommand : IRequest<Guid>
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
    
    // We get the OperatorId (UserId) from the current user's session
    public Guid OperatorId { get; set; } 
}

public class CreateBusCommandHandler : IRequestHandler<CreateBusCommand, Guid>
{
    private readonly IAppDbContext _context;

    public CreateBusCommandHandler(IAppDbContext context)
    {
        _context = context;
    }

    public async Task<Guid> Handle(CreateBusCommand request, CancellationToken cancellationToken)
    {
        // 1. Try to find the existing operator profile
        var busOperator = await _context.BusOperators
            .Include(o => o.User)
            .FirstOrDefaultAsync(o => o.UserId == request.OperatorId, cancellationToken);
            
        // 2. SELF-HEALING: If no profile exists but the user has the Operator role, create a profile now
        if (busOperator == null)
        {
            var user = await _context.Users.FindAsync(new object[] { request.OperatorId }, cancellationToken);
            

// Application/Buses/Commands/CreateBusCommand.cs
using BusBooking.Application.Commands;

namespace BusBooking.Application.Buses.Commands.CreateBus;

public class CreateBusCommand : BaseBusCommand
{
    public Guid OperatorId { get; set; }  // Required for operator
    
    protected override BusStatus GetInitialStatus() 
        => BusStatus.Pending;  // Operator's buses need approval
    
    protected override bool GetInitialAvailability() 
        => false;  // Not available until approved
    
    protected override Guid? GetOperatorId() 
        => OperatorId;  // Must be linked to operator account
}

// Handler with operator-specific validation
public class CreateBusCommandHandler : BaseBusCommandHandler<CreateBusCommand>
{
    public CreateBusCommandHandler(IAppDbContext context) : base(context) { }
    
    // Override to add operator validation
    protected override async Task BeforeCreateAsync(CreateBusCommand request, CancellationToken ct)
    {
        var busOperator = await _context.BusOperators
            .Include(o => o.User)
            .FirstOrDefaultAsync(o => o.UserId == request.OperatorId, ct);
        
        if (busOperator == null)
        {
            // Self-healing logic
            var user = await _context.Users.FindAsync(new object[] { request.OperatorId }, ct);
            if (user != null && user.Role == UserRole.BusOperator)
            {
                busOperator = new BusOperator
                {
                    Id = Guid.NewGuid(),
                    UserId = user.Id,
                    CompanyName = user.FullName + " Transport", // Defaulting to user's name
                    LicenseNumber = "PENDING",
                    Address = "Default Address",
                    Status = OperatorStatus.Approved, // Since they are already logged in as operator
                    CreatedAt = DateTime.UtcNow
                };
                
                _context.BusOperators.Add(busOperator);
                await _context.SaveChangesAsync(cancellationToken);
            }
        }

        if (busOperator == null) 
            throw new Exception($"Unauthorized: No operator profile linked to User ID {request.OperatorId}");

        var bus = new Bus
        {
            Id = Guid.NewGuid(),
            OperatorId = busOperator.Id,
            RouteId = request.RouteId,
            BusNumber = request.BusNumber,
            BusName = request.BusName,
            BusType = request.BusType,
            TotalSeats = request.TotalSeats,
            FemaleSeats = request.FemaleSeats,
            MaleSeats = request.MaleSeats,
            BasePrice = request.BasePrice,
            Amenities = request.Amenities,
            Status = BusStatus.Pending,
            IsAvailable = false, // Not available until approved
            CreatedAt = DateTime.UtcNow
        };

        _context.Buses.Add(bus);
        await _context.SaveChangesAsync(cancellationToken);

        return bus.Id;
    }
}

                    CompanyName = user.FullName + " Transport",
                    LicenseNumber = "PENDING",
                    Address = "Default Address",
                    Status = OperatorStatus.Approved,
                    CreatedAt = DateTime.UtcNow
                };
                _context.BusOperators.Add(busOperator);
                await _context.SaveChangesAsync(ct);
            }
        }
        
        if (busOperator == null)
            throw new Exception($"Unauthorized: No operator profile linked to User ID {request.OperatorId}");
    }
}

