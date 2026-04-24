using BusBooking.Domain.Common;

namespace BusBooking.Domain.Entities;

public class Station : BaseEntity
{
    public string Name { get; set; } = string.Empty;
    public string City { get; set; } = string.Empty;
    public string State { get; set; } = string.Empty;
    public string Code { get; set; } = string.Empty;  // e.g. "CHE", "BLR"
    public bool IsActive { get; set; } = true;

    // Navigation
    public ICollection<RouteStop> RouteStops { get; set; } = new List<RouteStop>();
}
