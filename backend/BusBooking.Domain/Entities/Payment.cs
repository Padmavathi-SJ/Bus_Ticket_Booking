using BusBooking.Domain.Common;
using BusBooking.Domain.Enums;

namespace BusBooking.Domain.Entities;

public class Payment : BaseEntity
{
    public Guid BookingId { get; set; }
    public decimal Amount { get; set; }
    public string Method { get; set; } = string.Empty;   // UPI, Card, NetBanking
    public string? TransactionId { get; set; }
    public PaymentStatus Status { get; set; } = PaymentStatus.Pending;
    public DateTime? PaidAt { get; set; }

    // Navigation
    public Booking Booking { get; set; } = null!;
}
