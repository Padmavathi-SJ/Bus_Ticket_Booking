using BusBooking.Domain.Common;
using BusBooking.Domain.Enums;

namespace BusBooking.Domain.Entities;

public class Booking : BaseEntity
{
    public Guid CustomerId { get; set; }
    public Guid TripId { get; set; }
    public DateTime BookingDate { get; set; } = DateTime.UtcNow;
    public decimal TotalAmount { get; set; }
    public BookingStatus Status { get; set; } = BookingStatus.Pending;
    public string? CancellationReason { get; set; }

    // Navigation
    public User Customer { get; set; } = null!;
    public Trip Trip { get; set; } = null!;
    public ICollection<BookingSeat> Seats { get; set; } = new List<BookingSeat>();
    public Payment? Payment { get; set; }
}
