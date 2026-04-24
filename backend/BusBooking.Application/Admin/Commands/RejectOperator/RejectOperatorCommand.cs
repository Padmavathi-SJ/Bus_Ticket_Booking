using BusBooking.Application.Common.Interfaces;
using BusBooking.Domain.Enums;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace BusBooking.Application.Admin.Commands.RejectOperator;

public record RejectOperatorCommand(Guid OperatorId, string Reason) : IRequest<bool>;

public class RejectOperatorCommandHandler : IRequestHandler<RejectOperatorCommand, bool>
{
    private readonly IAppDbContext _context;

    public RejectOperatorCommandHandler(IAppDbContext context)
    {
        _context = context;
    }

    public async Task<bool> Handle(RejectOperatorCommand request, CancellationToken cancellationToken)
    {
        var busOperator = await _context.BusOperators
            .Include(o => o.User)
            .FirstOrDefaultAsync(o => o.Id == request.OperatorId, cancellationToken);

        if (busOperator == null) return false;

        busOperator.Status = OperatorStatus.Rejected;
        busOperator.RejectionReason = request.Reason;
        busOperator.User.IsActive = false; // Keep user inactive if rejected

        await _context.SaveChangesAsync(cancellationToken);
        return true;
    }
}
