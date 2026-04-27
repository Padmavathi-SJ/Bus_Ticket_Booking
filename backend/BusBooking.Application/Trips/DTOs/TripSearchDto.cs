namespace BusBooking.Application.Trips.DTOs;

public class TripSearchDto
{
    // Trip identifiers
    public Guid TripId { get; set; }
    public Guid BusId { get; set; }
    
    // Bus details
    public string BusName { get; set; } = string.Empty;
    public string BusNumber { get; set; } = string.Empty;
    public string BusType { get; set; } = string.Empty;
    public string OperatorName { get; set; } = string.Empty;
    public int TotalSeats { get; set; }
    public int FemaleSeats { get; set; }
    public int MaleSeats { get; set; }
    public string Amenities { get; set; } = string.Empty;
    
    // Trip details
    public DateTime TripDate { get; set; }
    public string SourceAddress { get; set; } = string.Empty;
    public string DestinationAddress { get; set; } = string.Empty;
    public string PickupPoint { get; set; } = string.Empty;
    public string DropPoint { get; set; } = string.Empty;
    public string DepartureTime { get; set; } = string.Empty;
    public string ArrivalTime { get; set; } = string.Empty;
    public int Duration { get; set; } // in minutes
    
    // Pricing & availability
    public decimal BasePrice { get; set; }
    public int BookedSeats { get; set; }
    public int AvailableSeats { get; set; }
    public bool IsAvailable { get; set; } // Bus availability status
}
