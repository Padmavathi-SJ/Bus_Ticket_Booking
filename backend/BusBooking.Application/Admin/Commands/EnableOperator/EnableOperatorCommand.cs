using BusBooking.Application.Common.Interfaces;
using BusBooking.Domain.Enums;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace BusBooking.Application.Admin.Commands.EnableOperator;

public record EnableOperatorCommand(Guid OperatorId) : IRequest<bool>;

public class EnableOperatorCommandHandler : IRequestHandler<EnableOperatorCommand, bool>
{
    private readonly IAppDbContext _context;
    private readonly IEmailService _emailService;

    public EnableOperatorCommandHandler(IAppDbContext context, IEmailService emailService)
    {
        _context = context;
        _emailService = emailService;
    }

    public async Task<bool> Handle(EnableOperatorCommand request, CancellationToken cancellationToken)
    {
        // Get the operator with user details
        var operatorEntity = await _context.BusOperators
            .Include(o => o.User)
            .Include(o => o.Buses)
            .FirstOrDefaultAsync(o => o.Id == request.OperatorId, cancellationToken);

        if (operatorEntity == null)
            return false;

        // Update operator status to Approved (enabled)
        operatorEntity.Status = OperatorStatus.Approved;

        // Enable all buses owned by this operator
        foreach (var bus in operatorEntity.Buses)
        {
            bus.IsAvailable = true;
        }

        // Send email notification to operator
        await _emailService.SendEmailAsync(
            operatorEntity.User.Email,
            "Your Operator Account Has Been Enabled",
            $@"
            <html>
            <body style='font-family: Arial, sans-serif; line-height: 1.6; color: #333;'>
                <div style='max-width: 600px; margin: 0 auto; padding: 20px; border: 1px solid #ddd; border-radius: 8px;'>
                    <h2 style='color: #10b981;'>Account Enabled</h2>
                    <p>Dear {operatorEntity.User.FullName},</p>
                    <p>Great news! Your bus operator account has been enabled by the administration.</p>
                    
                    <div style='background: #f0fdf4; padding: 15px; border-radius: 6px; margin: 20px 0; border-left: 4px solid #10b981;'>
                        <h3 style='margin-top: 0; color: #10b981;'>What This Means</h3>
                        <ul>
                            <li>All your buses are now available for booking</li>
                            <li>Customers can search and book your buses</li>
                            <li>You can manage your fleet and trips normally</li>
                        </ul>
                    </div>

                    <p>You can now continue operating your bus services. Thank you for your patience!</p>
                    
                    <p>Best regards,<br/>Bus Booking System Team</p>
                </div>
            </body>
            </html>
            "
        );

        await _context.SaveChangesAsync(cancellationToken);
        return true;
    }
}
