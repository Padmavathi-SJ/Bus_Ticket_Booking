using BusBooking.Application.Common.Interfaces;
using BusBooking.Domain.Enums;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace BusBooking.Application.Admin.Commands.ApproveBus;

public record ApproveBusCommand(Guid BusId) : IRequest<bool>;

public class ApproveBusCommandHandler : IRequestHandler<ApproveBusCommand, bool>
{
    private readonly IAppDbContext _context;

    public ApproveBusCommandHandler(IAppDbContext context)
    {
        _context = context;
    }

    public async Task<bool> Handle(ApproveBusCommand request, CancellationToken cancellationToken)
    {
        var bus = await _context.Buses
            .FirstOrDefaultAsync(b => b.Id == request.BusId, cancellationToken);
        if (bus == null) return false;

        bus.Status = BusStatus.Approved;
        bus.IsAvailable = true;
        bus.ApprovedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync(cancellationToken);
        return true;
    }
}
