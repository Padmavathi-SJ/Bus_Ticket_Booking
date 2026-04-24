using BusBooking.Domain.Enums;

namespace BusBooking.Application.Bookings.DTOs;

public class BookingDto
{
    public Guid Id { get; set; }
    public Guid BusId { get; set; }
    public string BusName { get; set; } = string.Empty;
    public string BusNumber { get; set; } = string.Empty;
    public string OperatorName { get; set; } = string.Empty;
    public string Source { get; set; } = string.Empty;
    public string Destination { get; set; } = string.Empty;
    public List<string> SeatNumbers { get; set; } = new();
    public string PassengerName { get; set; } = string.Empty;
    public string PassengerEmail { get; set; } = string.Empty;
    public string PassengerPhone { get; set; } = string.Empty;
    public decimal TotalAmount { get; set; }
    public DateTime BookingDate { get; set; }
    public DateTime JourneyDate { get; set; }
    public string Status { get; set; } = string.Empty;
}

public class CreateBookingDto
{
    public Guid? BusId { get; set; } // Optional for backward compatibility
    public Guid? TripId { get; set; } // New field for trip-based bookings
    public List<string> SeatNumbers { get; set; } = new();
    public PassengerDetailsDto PassengerDetails { get; set; } = new();
    public decimal TotalAmount { get; set; }
}

public class PassengerDetailsDto
{
    public string FullName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string Phone { get; set; } = string.Empty;
    public int Age { get; set; }
    public string Gender { get; set; } = string.Empty;
}

public class BookingConfirmationDto
{
    public Guid Id { get; set; }
    public string BookingReference { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
    public string Message { get; set; } = string.Empty;
}

public class OperatorBookingDto
{
    public Guid Id { get; set; }
    public string BookingReference { get; set; } = string.Empty;
    public Guid BusId { get; set; }
    public string BusName { get; set; } = string.Empty;
    public string BusNumber { get; set; } = string.Empty;
    public string BusType { get; set; } = string.Empty;
    public string Source { get; set; } = string.Empty;
    public string Destination { get; set; } = string.Empty;
    public List<string> SeatNumbers { get; set; } = new();
    public string PassengerName { get; set; } = string.Empty;
    public string PassengerEmail { get; set; } = string.Empty;
    public string PassengerPhone { get; set; } = string.Empty;
    public int PassengerAge { get; set; }
    public string PassengerGender { get; set; } = string.Empty;
    public decimal TotalAmount { get; set; }
    public DateTime BookingDate { get; set; }
    public DateTime JourneyDate { get; set; }
    public string Status { get; set; } = string.Empty;
}
