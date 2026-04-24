using BusBooking.Application.Bookings.DTOs;
using BusBooking.Application.Common.Interfaces;
using BusBooking.Domain.Entities;
using BusBooking.Domain.Enums;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace BusBooking.Application.Bookings.Commands.CreateBooking;

public record CreateBookingCommand(
    Guid CustomerId,
    Guid? BusId,
    Guid? TripId,
    List<string> SeatNumbers,
    PassengerDetailsDto PassengerDetails,
    decimal TotalAmount
) : IRequest<BookingConfirmationDto>;

public class CreateBookingCommandHandler : IRequestHandler<CreateBookingCommand, BookingConfirmationDto>
{
    private readonly IAppDbContext _context;
    private readonly IEmailService _emailService;

    public CreateBookingCommandHandler(IAppDbContext context, IEmailService emailService)
    {
        _context = context;
        _emailService = emailService;
    }

    public async Task<BookingConfirmationDto> Handle(CreateBookingCommand request, CancellationToken cancellationToken)
    {
        Trip trip;
        Guid busId;

        // If TripId is provided, use it directly (new trip-based booking)
        if (request.TripId.HasValue)
        {
            trip = await _context.Trips
                .Include(t => t.Bus)
                    .ThenInclude(b => b.SeatLayouts)
                .FirstOrDefaultAsync(t => t.Id == request.TripId.Value 
                    && t.Status == TripStatus.Scheduled, cancellationToken);

            if (trip == null)
            {
                throw new Exception("Trip not found or not available");
            }

            busId = trip.BusId;

            // Validate bus is approved and available
            if (trip.Bus.Status != BusStatus.Approved || !trip.Bus.IsAvailable)
            {
                throw new Exception("Bus not available for booking");
            }
        }
        // Otherwise, use BusId (legacy bus-based booking)
        else if (request.BusId.HasValue)
        {
            // Validate bus exists and is available
            var bus = await _context.Buses
                .Include(b => b.SeatLayouts)
                .FirstOrDefaultAsync(b => b.Id == request.BusId.Value 
                    && b.Status == BusStatus.Approved 
                    && b.IsAvailable, cancellationToken);

            if (bus == null)
            {
                throw new Exception("Bus not found or not available");
            }

            busId = request.BusId.Value;

            // Get or create a default trip for today
            var today = DateTime.UtcNow.Date;
            trip = await _context.Trips
                .FirstOrDefaultAsync(t => t.BusId == busId 
                    && t.DepartureDateTime.Date == today
                    && t.Status == TripStatus.Scheduled, cancellationToken);

            if (trip == null)
            {
                // Create a default trip for simplified booking
                trip = new Trip
                {
                    Id = Guid.NewGuid(),
                    BusId = busId,
                    RouteId = bus.RouteId,
                    DepartureDateTime = today.AddHours(8), // Default 8 AM
                    ArrivalDateTime = today.AddHours(16), // Default 4 PM
                    Status = TripStatus.Scheduled,
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow
                };
                _context.Trips.Add(trip);
            }
        }
        else
        {
            throw new Exception("Either BusId or TripId must be provided");
        }

        // Check if seats are already booked for this trip
        // Use a transaction to prevent race conditions
        using var transaction = await _context.Database.BeginTransactionAsync(cancellationToken);

        try
        {
            var existingBookings = await _context.Bookings
                .Include(b => b.Seats)
                .ThenInclude(s => s.SeatLayout)
                .Where(b => b.TripId == trip.Id 
                    && b.Status != BookingStatus.Cancelled)
                .SelectMany(b => b.Seats.Select(s => s.SeatLayout.SeatLabel))
                .ToListAsync(cancellationToken);

            var alreadyBooked = request.SeatNumbers.Intersect(existingBookings).ToList();
            if (alreadyBooked.Any())
            {
                await transaction.RollbackAsync(cancellationToken);
                throw new Exception($"Seats already booked: {string.Join(", ", alreadyBooked)}");
            }

            // Create booking
            var booking = new Booking
            {
                Id = Guid.NewGuid(),
                CustomerId = request.CustomerId,
                TripId = trip.Id,
                BookingDate = DateTime.UtcNow,
                TotalAmount = request.TotalAmount,
                Status = BookingStatus.Confirmed,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };

            _context.Bookings.Add(booking);

            // Create booking seats
            foreach (var seatNumber in request.SeatNumbers)
            {
                // Get or create seat layout
                var seatLayout = await _context.SeatLayouts
                    .FirstOrDefaultAsync(s => s.BusId == busId 
                        && s.SeatLabel == seatNumber, cancellationToken);

                if (seatLayout == null)
                {
                    // Create seat layout if it doesn't exist
                    seatLayout = new SeatLayout
                    {
                        Id = Guid.NewGuid(),
                        BusId = busId,
                        RowNumber = int.Parse(seatNumber) / 4 + 1,
                        ColumnLabel = ((char)('A' + (int.Parse(seatNumber) - 1) % 4)).ToString(),
                        SeatLabel = seatNumber,
                        SeatType = SeatType.Seater,
                        IsActive = true,
                        CreatedAt = DateTime.UtcNow,
                        UpdatedAt = DateTime.UtcNow
                    };
                    _context.SeatLayouts.Add(seatLayout);
                    await _context.SaveChangesAsync(cancellationToken);
                }

                var bookingSeat = new BookingSeat
                {
                    Id = Guid.NewGuid(),
                    BookingId = booking.Id,
                    SeatLayoutId = seatLayout.Id,
                    PassengerName = request.PassengerDetails.FullName,
                    Age = request.PassengerDetails.Age,
                    Gender = request.PassengerDetails.Gender,
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow
                };

                _context.BookingSeats.Add(bookingSeat);
            }

            await _context.SaveChangesAsync(cancellationToken);
            await transaction.CommitAsync(cancellationToken);

            var bookingReference = $"BK{booking.Id.ToString()[..8].ToUpper()}";

            // Load full trip details for email
            var tripWithDetails = await _context.Trips
                .Include(t => t.Bus)
                    .ThenInclude(b => b.Operator)
                .Include(t => t.Route)
                .FirstOrDefaultAsync(t => t.Id == trip.Id, cancellationToken);

            // Send confirmation email (fire and forget - don't block the response)
            _ = Task.Run(async () =>
            {
                try
                {
                    await _emailService.SendBookingConfirmationEmailAsync(
                        toEmail: request.PassengerDetails.Email,
                        passengerName: request.PassengerDetails.FullName,
                        bookingReference: bookingReference,
                        busName: tripWithDetails?.Bus.BusName ?? "N/A",
                        busNumber: tripWithDetails?.Bus.BusNumber ?? "N/A",
                        source: tripWithDetails?.SourceAddress ?? tripWithDetails?.Route?.Source ?? "N/A",
                        destination: tripWithDetails?.DestinationAddress ?? tripWithDetails?.Route?.Destination ?? "N/A",
                        pickupPoint: tripWithDetails?.PickupPoint ?? "Main Pickup Point",
                        dropPoint: tripWithDetails?.DropPoint ?? "Main Drop Point",
                        tripDate: tripWithDetails?.DepartureDateTime.Date ?? DateTime.UtcNow.Date,
                        departureTime: tripWithDetails?.DepartureDateTime.ToString("HH:mm") ?? "N/A",
                        arrivalTime: tripWithDetails?.ArrivalDateTime.ToString("HH:mm") ?? "N/A",
                        seatNumbers: request.SeatNumbers,
                        totalAmount: request.TotalAmount,
                        cancellationToken: CancellationToken.None);
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"[EMAIL-ERROR] Failed to send booking confirmation email: {ex.Message}");
                }
            }, CancellationToken.None);

            return new BookingConfirmationDto
            {
                Id = booking.Id,
                BookingReference = bookingReference,
                Status = "Confirmed",
                Message = "Booking confirmed successfully"
            };
        }
        catch (Exception)
        {
            await transaction.RollbackAsync(cancellationToken);
            throw;
        }
    }
}
