using BusBooking.Domain.Common;
using BusBooking.Domain.Enums;

namespace BusBooking.Domain.Entities;

public class Bus : BaseEntity
{
    public Guid? OperatorId { get; set; } // Nullable if added by Admin directly
    public Guid RouteId { get; set; }
    public string BusNumber { get; set; } = string.Empty;
    public string BusName { get; set; } = string.Empty;
    public string BusType { get; set; } = string.Empty; // e.g. "AC Sleeper", "Non-AC Seater"
    public int TotalSeats { get; set; }
    public double BasePrice { get; set; }
    public string Amenities { get; set; } = string.Empty; // e.g. "Wifi, Water, Charging Point"
    public string? Description { get; set; }
    public BusStatus Status { get; set; } = BusStatus.Pending;
    public bool IsAvailable { get; set; } = true;
    public int FemaleSeats { get; set; }
    public int MaleSeats { get; set; }
    
    // Approval tracking
    public Guid? ApprovedBy { get; set; }
    public DateTime? ApprovedAt { get; set; }
    public string? RejectionReason { get; set; }

    // Navigation
    public BusOperator? Operator { get; set; }
    public BusRoute Route { get; set; } = null!;
    public ICollection<SeatLayout> SeatLayouts { get; set; } = new List<SeatLayout>();
    public ICollection<Trip> Trips { get; set; } = new List<Trip>();
}
