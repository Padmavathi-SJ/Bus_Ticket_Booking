using BusBooking.Domain.Common;
using BusBooking.Domain.Enums;

namespace BusBooking.Domain.Entities;

public class Trip : BaseEntity
{
    public Guid BusId { get; set; }
    public Guid RouteId { get; set; }
    public DateTime DepartureDateTime { get; set; }
    public DateTime ArrivalDateTime { get; set; }
    public TripStatus Status { get; set; } = TripStatus.Scheduled;
    
    // Additional trip details for operator scheduling
    public string? SourceAddress { get; set; }
    public string? DestinationAddress { get; set; }
    public string? PickupPoint { get; set; }
    public string? DropPoint { get; set; }

    // Navigation
    public Bus Bus { get; set; } = null!;
    public BusRoute Route { get; set; } = null!;
    public ICollection<TripPricing> Pricing { get; set; } = new List<TripPricing>();
    public ICollection<Booking> Bookings { get; set; } = new List<Booking>();
}
