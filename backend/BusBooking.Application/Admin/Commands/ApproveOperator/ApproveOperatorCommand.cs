using BusBooking.Application.Common.Interfaces;
using BusBooking.Domain.Enums;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace BusBooking.Application.Admin.Commands.ApproveOperator;

public record ApproveOperatorCommand(Guid OperatorId) : IRequest<bool>;

public class ApproveOperatorCommandHandler : IRequestHandler<ApproveOperatorCommand, bool>
{
    private readonly IAppDbContext _context;

    public ApproveOperatorCommandHandler(IAppDbContext context)
    {
        _context = context;
    }

    public async Task<bool> Handle(ApproveOperatorCommand request, CancellationToken cancellationToken)
    {
        var busOperator = await _context.BusOperators
            .Include(o => o.User)
            .FirstOrDefaultAsync(o => o.Id == request.OperatorId, cancellationToken);

        if (busOperator == null) return false;

        busOperator.Status = OperatorStatus.Approved;
        busOperator.ApprovedAt = DateTime.UtcNow;
        busOperator.User.IsActive = true; // Ensure user is active upon approval

        await _context.SaveChangesAsync(cancellationToken);
        return true;
    }
}
