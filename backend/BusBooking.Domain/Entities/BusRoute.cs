using BusBooking.Domain.Common;

namespace BusBooking.Domain.Entities;

public class BusRoute : BaseEntity
{
    public string Name { get; set; } = string.Empty;
    public string Source { get; set; } = string.Empty;
    public string Destination { get; set; } = string.Empty;
    public double DistanceKm { get; set; }
    public bool IsActive { get; set; } = true;

    // Navigation
    public ICollection<RouteStop> Stops { get; set; } = new List<RouteStop>();
    public ICollection<Trip> Trips { get; set; } = new List<Trip>();
}
