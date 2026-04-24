namespace BusBooking.Application.Routes.DTOs;

public class RouteDto
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Source { get; set; } = string.Empty;
    public string Destination { get; set; } = string.Empty;
    public double DistanceKm { get; set; }
    public bool IsActive { get; set; }
}
