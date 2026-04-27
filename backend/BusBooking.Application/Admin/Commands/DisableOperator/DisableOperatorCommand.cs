using BusBooking.Application.Common.Interfaces;
using BusBooking.Domain.Enums;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace BusBooking.Application.Admin.Commands.DisableOperator;

public record DisableOperatorCommand(Guid OperatorId) : IRequest<bool>;

public class DisableOperatorCommandHandler : IRequestHandler<DisableOperatorCommand, bool>
{
    private readonly IAppDbContext _context;
    private readonly IEmailService _emailService;

    public DisableOperatorCommandHandler(IAppDbContext context, IEmailService emailService)
    {
        _context = context;
        _emailService = emailService;
    }

    public async Task<bool> Handle(DisableOperatorCommand request, CancellationToken cancellationToken)
    {
        // Get the operator with user details
        var operatorEntity = await _context.BusOperators
            .Include(o => o.User)
            .Include(o => o.Buses)
                .ThenInclude(b => b.Trips)
                    .ThenInclude(t => t.Bookings)
                        .ThenInclude(b => b.Customer)
            .FirstOrDefaultAsync(o => o.Id == request.OperatorId, cancellationToken);

        if (operatorEntity == null)
            return false;

        // Update operator status to Disabled
        operatorEntity.Status = OperatorStatus.Disabled;

        // Disable all buses owned by this operator
        foreach (var bus in operatorEntity.Buses)
        {
            bus.IsAvailable = false;
        }

        // Get all active bookings for this operator's buses
        var affectedBookings = operatorEntity.Buses
            .SelectMany(b => b.Trips)
            .SelectMany(t => t.Bookings)
            .Where(b => b.Status == BookingStatus.Confirmed)
            .ToList();

        // Cancel all active bookings and notify customers
        foreach (var booking in affectedBookings)
        {
            booking.Status = BookingStatus.Cancelled;
            booking.CancellationReason = "Bus operator has been temporarily disabled by admin";

            // Send email notification to customer
            var customerEmail = booking.Customer.Email;
            var customerName = booking.Customer.FullName;
            var busName = booking.Trip.Bus.BusName;
            var tripDate = booking.Trip.DepartureDateTime.ToString("dd MMM yyyy");

            await _emailService.SendEmailAsync(
                customerEmail,
                "Booking Cancelled - Bus Temporarily Unavailable",
                $@"
                <html>
                <body style='font-family: Arial, sans-serif; line-height: 1.6; color: #333;'>
                    <div style='max-width: 600px; margin: 0 auto; padding: 20px; border: 1px solid #ddd; border-radius: 8px;'>
                        <h2 style='color: #ef4444;'>Booking Cancelled</h2>
                        <p>Dear {customerName},</p>
                        <p>We regret to inform you that your booking has been cancelled due to the bus operator being temporarily disabled by the administration.</p>
                        
                        <div style='background: #f9fafb; padding: 15px; border-radius: 6px; margin: 20px 0;'>
                            <h3 style='margin-top: 0; color: #667eea;'>Booking Details</h3>
                            <p><strong>Booking Reference:</strong> {booking.Id.ToString().Substring(0, 8).ToUpper()}</p>
                            <p><strong>Bus:</strong> {busName}</p>
                            <p><strong>Trip Date:</strong> {tripDate}</p>
                            <p><strong>Amount:</strong> ₹{booking.TotalAmount:F2}</p>
                        </div>

                        <p><strong>Refund Information:</strong></p>
                        <p>The full amount of <strong>₹{booking.TotalAmount:F2}</strong> will be refunded to your account within 5-7 business days.</p>
                        
                        <p>We apologize for any inconvenience caused. Please feel free to book another bus for your journey.</p>
                        
                        <p>Best regards,<br/>Bus Booking System Team</p>
                    </div>
                </body>
                </html>
                "
            );
        }

        // Send email notification to operator
        await _emailService.SendEmailAsync(
            operatorEntity.User.Email,
            "Your Operator Account Has Been Disabled",
            $@"
            <html>
            <body style='font-family: Arial, sans-serif; line-height: 1.6; color: #333;'>
                <div style='max-width: 600px; margin: 0 auto; padding: 20px; border: 1px solid #ddd; border-radius: 8px;'>
                    <h2 style='color: #ef4444;'>Account Disabled</h2>
                    <p>Dear {operatorEntity.User.FullName},</p>
                    <p>Your bus operator account has been temporarily disabled by the administration.</p>
                    
                    <div style='background: #fef2f2; padding: 15px; border-radius: 6px; margin: 20px 0; border-left: 4px solid #ef4444;'>
                        <h3 style='margin-top: 0; color: #ef4444;'>Impact</h3>
                        <ul>
                            <li>All your buses are now temporarily unavailable for booking</li>
                            <li>Active bookings ({affectedBookings.Count}) have been cancelled</li>
                            <li>Customers will be refunded automatically</li>
                        </ul>
                    </div>

                    <p>If you believe this is a mistake or need clarification, please contact the administration immediately.</p>
                    
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
