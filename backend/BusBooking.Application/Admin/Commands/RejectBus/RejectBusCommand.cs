using BusBooking.Application.Common.Interfaces;
using BusBooking.Domain.Enums;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace BusBooking.Application.Admin.Commands.RejectBus;

public record RejectBusCommand(Guid BusId, string Reason) : IRequest<bool>;

public class RejectBusCommandHandler : IRequestHandler<RejectBusCommand, bool>
{
    private readonly IAppDbContext _context;

    public RejectBusCommandHandler(IAppDbContext context)
    {
        _context = context;
    }

    public async Task<bool> Handle(RejectBusCommand request, CancellationToken cancellationToken)
    {
        var bus = await _context.Buses.FindAsync(new object[] { request.BusId }, cancellationToken);
        if (bus == null) return false;

        bus.Status = BusStatus.Rejected;
        bus.RejectionReason = request.Reason;
        bus.IsAvailable = false;

        // Note: In a full implementation, we would create a Notification entity here
        // for the operator.
        
        await _context.SaveChangesAsync(cancellationToken);
        return true;
    }
}
