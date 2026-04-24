namespace BusBooking.Application.Trips.DTOs;

public class TripDto
{
    public Guid Id { get; set; }
    public DateTime TripDate { get; set; }
    public string SourceAddress { get; set; } = string.Empty;
    public string DestinationAddress { get; set; } = string.Empty;
    public string PickupPoint { get; set; } = string.Empty;
    public string DropPoint { get; set; } = string.Empty;
    public string DepartureTime { get; set; } = string.Empty;
    public string ArrivalTime { get; set; } = string.Empty;
    public decimal BasePrice { get; set; }
    public string Status { get; set; } = string.Empty;
    public int BookedSeats { get; set; }
    public int AvailableSeats { get; set; }
}
