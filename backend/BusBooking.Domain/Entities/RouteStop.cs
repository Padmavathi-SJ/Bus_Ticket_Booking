using BusBooking.Domain.Common;

namespace BusBooking.Domain.Entities;

public class RouteStop : BaseEntity
{
    public Guid RouteId { get; set; }
    public Guid StationId { get; set; }
    public int StopOrder { get; set; }
    public int ArrivalOffsetMinutes { get; set; }

    // Navigation
    public BusRoute Route { get; set; } = null!;
    public Station Station { get; set; } = null!;
}
