using BusBooking.Domain.Common;
using BusBooking.Domain.Enums;

namespace BusBooking.Domain.Entities;

public class SeatLayout : BaseEntity
{
    public Guid BusId { get; set; }
    public int RowNumber { get; set; }
    public string ColumnLabel { get; set; } = string.Empty; // A, B, C, D
    public string SeatLabel { get; set; } = string.Empty;  // 1A, 1B, 2A...
    public SeatType SeatType { get; set; }
    public bool IsActive { get; set; } = true;

    // Navigation
    public Bus Bus { get; set; } = null!;
    public ICollection<BookingSeat> BookingSeats { get; set; } = new List<BookingSeat>();
}
