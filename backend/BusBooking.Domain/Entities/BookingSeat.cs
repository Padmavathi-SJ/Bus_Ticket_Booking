using BusBooking.Domain.Common;

namespace BusBooking.Domain.Entities;

public class BookingSeat : BaseEntity
{
    public Guid BookingId { get; set; }
    public Guid SeatLayoutId { get; set; }
    public string PassengerName { get; set; } = string.Empty;
    public int Age { get; set; }
    public string Gender { get; set; } = string.Empty;

    // Navigation
    public Booking Booking { get; set; } = null!;
    public SeatLayout SeatLayout { get; set; } = null!;
}
