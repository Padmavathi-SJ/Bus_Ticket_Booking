namespace BusBooking.Application.Common.Interfaces;

public interface IEmailService
{
    Task SendBookingConfirmationEmailAsync(
        string toEmail,
        string passengerName,
        string bookingReference,
        string busName,
        string busNumber,
        string source,
        string destination,
        string pickupPoint,
        string dropPoint,
        DateTime tripDate,
        string departureTime,
        string arrivalTime,
        List<string> seatNumbers,
        decimal totalAmount,
        CancellationToken cancellationToken = default);

    Task SendBookingCancellationEmailAsync(
        string toEmail,
        string passengerName,
        string bookingReference,
        string busName,
        string busNumber,
        string source,
        string destination,
        DateTime tripDate,
        List<string> seatNumbers,
        decimal refundAmount,
        CancellationToken cancellationToken = default);
}
