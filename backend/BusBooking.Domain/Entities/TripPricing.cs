using BusBooking.Domain.Common;
using BusBooking.Domain.Enums;

namespace BusBooking.Domain.Entities;

public class TripPricing : BaseEntity
{
    public Guid TripId { get; set; }
    public SeatType SeatType { get; set; }
    public decimal Price { get; set; }

    // Navigation
    public Trip Trip { get; set; } = null!;
}
