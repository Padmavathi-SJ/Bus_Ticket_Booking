namespace BusBooking.Domain.Entities;

public class Notification
{
    public Guid Id { get; set; }
    public Guid UserId { get; set; } // Operator ID
    public string Title { get; set; } = string.Empty;
    public string Message { get; set; } = string.Empty;
    public string Type { get; set; } = string.Empty; // "BookingCancellation", "NewBooking", etc.
    public bool IsRead { get; set; } = false;
    public Guid? RelatedBookingId { get; set; }
    public DateTime CreatedAt { get; set; }
    
    // Navigation properties
    public User User { get; set; } = null!;
    public Booking? RelatedBooking { get; set; }
}
