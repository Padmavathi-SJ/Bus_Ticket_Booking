using BusBooking.Domain.Enums;

namespace BusBooking.Application.Buses.DTOs;

public class BusDto
{
    public Guid Id { get; set; }
    public Guid? OperatorId { get; set; }
    public string? OperatorName { get; set; }
    public Guid RouteId { get; set; }
    public string? RouteName { get; set; }
    public string BusNumber { get; set; } = string.Empty;
    public string BusName { get; set; } = string.Empty;
    public string BusType { get; set; } = string.Empty;
    public int TotalSeats { get; set; }
    public double BasePrice { get; set; }
    public string Amenities { get; set; } = string.Empty;
    public BusStatus Status { get; set; }
    public bool IsAvailable { get; set; }
    public string? Source { get; set; }
    public string? Destination { get; set; }
    public int FemaleSeats { get; set; }
    public int MaleSeats { get; set; }
}
