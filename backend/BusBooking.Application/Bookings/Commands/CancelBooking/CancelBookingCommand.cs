using BusBooking.Application.Common.Interfaces;
using BusBooking.Domain.Enums;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace BusBooking.Application.Bookings.Commands.CancelBooking;

public record CancelBookingCommand(Guid BookingId, Guid CustomerId) : IRequest<CancelBookingResult>;

public class CancelBookingResult
{
    public string Message { get; set; } = string.Empty;
    public decimal RefundAmount { get; set; }
}

public class CancelBookingCommandHandler : IRequestHandler<CancelBookingCommand, CancelBookingResult>
{
    private readonly IAppDbContext _context;
    private readonly IEmailService _emailService;

    public CancelBookingCommandHandler(IAppDbContext context, IEmailService emailService)
    {
        _context = context;
        _emailService = emailService;
    }

    public async Task<CancelBookingResult> Handle(CancelBookingCommand request, CancellationToken cancellationToken)
    {
        var booking = await _context.Bookings
            .Include(b => b.Trip)
                .ThenInclude(t => t.Bus)
                    .ThenInclude(b => b.Operator)
                        .ThenInclude(o => o.User)
            .Include(b => b.Trip.Route)
            .Include(b => b.Seats)
                .ThenInclude(s => s.SeatLayout)
            .FirstOrDefaultAsync(b => b.Id == request.BookingId, cancellationToken);

        if (booking == null)
        {
            throw new Exception("Booking not found");
        }

        if (booking.CustomerId != request.CustomerId)
        {
            throw new UnauthorizedAccessException("You can only cancel your own bookings");
        }

        if (booking.Status == BookingStatus.Cancelled)
        {
            throw new Exception("Booking is already cancelled");
        }

        if (booking.Status != BookingStatus.Confirmed)
        {
            throw new Exception("Only confirmed bookings can be cancelled");
        }

        // Get passenger details from booking seats
        var passengerName = booking.Seats.FirstOrDefault()?.PassengerName ?? "Passenger";
        var seatNumbers = booking.Seats.Select(s => s.SeatLayout.SeatLabel).ToList();
        
        // Get customer email
        var customer = await _context.Users.FindAsync(new object[] { request.CustomerId }, cancellationToken);
        var customerEmail = customer?.Email ?? "";

        booking.Status = BookingStatus.Cancelled;
        booking.CancellationReason = "Cancelled by customer";
        booking.UpdatedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync(cancellationToken);

        var bookingReference = $"BK{booking.Id.ToString()[..8].ToUpper()}";

        // Create notification for operator
        if (booking.Trip?.Bus?.Operator?.UserId != null)
        {
            var notification = new Domain.Entities.Notification
            {
                Id = Guid.NewGuid(),
                UserId = booking.Trip.Bus.Operator.UserId,
                Title = "Booking Cancelled",
                Message = $"Booking {bookingReference} for {booking.Trip.Bus.BusName} on {booking.Trip.DepartureDateTime:MMM dd, yyyy} has been cancelled by the customer. Seats: {string.Join(", ", seatNumbers)}",
                Type = "BookingCancellation",
                IsRead = false,
                RelatedBookingId = booking.Id,
                CreatedAt = DateTime.UtcNow
            };
            
            _context.Notifications.Add(notification);
            await _context.SaveChangesAsync(cancellationToken);
            
            Console.WriteLine($"[NOTIFICATION] Created cancellation notification for operator user {booking.Trip.Bus.Operator.UserId}");
        }

        // Send cancellation email to customer (fire and forget)
        if (!string.IsNullOrEmpty(customerEmail))
        {
            _ = Task.Run(async () =>
            {
                try
                {
                    await _emailService.SendBookingCancellationEmailAsync(
                        toEmail: customerEmail,
                        passengerName: passengerName,
                        bookingReference: bookingReference,
                        busName: booking.Trip?.Bus?.BusName ?? "N/A",
                        busNumber: booking.Trip?.Bus?.BusNumber ?? "N/A",
                        source: booking.Trip?.SourceAddress ?? booking.Trip?.Route?.Source ?? "N/A",
                        destination: booking.Trip?.DestinationAddress ?? booking.Trip?.Route?.Destination ?? "N/A",
                        tripDate: booking.Trip?.DepartureDateTime.Date ?? DateTime.UtcNow.Date,
                        seatNumbers: seatNumbers,
                        refundAmount: booking.TotalAmount,
                        cancellationToken: CancellationToken.None);
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"[EMAIL-CANCEL-ERROR] Failed to send cancellation email: {ex.Message}");
                }
            }, CancellationToken.None);
        }

        return new CancelBookingResult
        {
            Message = "Booking cancelled successfully",
            RefundAmount = booking.TotalAmount
        };
    }
}
