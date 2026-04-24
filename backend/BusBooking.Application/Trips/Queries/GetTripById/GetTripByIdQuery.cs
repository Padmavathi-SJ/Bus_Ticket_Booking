using BusBooking.Application.Common.Interfaces;
using BusBooking.Application.Trips.DTOs;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace BusBooking.Application.Trips.Queries.GetTripById;

public record GetTripByIdQuery(Guid TripId) : IRequest<TripDetailsDto>;

public class GetTripByIdQueryHandler : IRequestHandler<GetTripByIdQuery, TripDetailsDto>
{
    private readonly IAppDbContext _context;

    public GetTripByIdQueryHandler(IAppDbContext context)
    {
        _context = context;
    }

    public async Task<TripDetailsDto> Handle(GetTripByIdQuery request, CancellationToken cancellationToken)
    {
        var trip = await _context.Trips
            .Include(t => t.Bus)
                .ThenInclude(b => b.Operator)
            .Include(t => t.Route)
            .Include(t => t.Bookings)
            .Include(t => t.Pricing)
            .Where(t => t.Id == request.TripId)
            .Select(t => new TripDetailsDto
            {
                TripId = t.Id,
                BusId = t.BusId,
                BusName = t.Bus.BusName,
                BusNumber = t.Bus.BusNumber,
                BusType = t.Bus.BusType,
                OperatorName = t.Bus.Operator != null ? t.Bus.Operator.CompanyName : "Unknown",
                TotalSeats = t.Bus.TotalSeats,
                FemaleSeats = t.Bus.FemaleSeats,
                MaleSeats = t.Bus.MaleSeats,
                Amenities = t.Bus.Amenities,
                
                // Trip specific details
                TripDate = t.DepartureDateTime.Date,
                SourceAddress = t.SourceAddress ?? t.Route.Source,
                DestinationAddress = t.DestinationAddress ?? t.Route.Destination,
                PickupPoint = t.PickupPoint ?? "Main Pickup Point",
                DropPoint = t.DropPoint ?? "Main Drop Point",
                DepartureTime = t.DepartureDateTime.ToString("HH:mm"),
                ArrivalTime = t.ArrivalDateTime.ToString("HH:mm"),
                Duration = (int)(t.ArrivalDateTime - t.DepartureDateTime).TotalMinutes,
                
                // Pricing
                BasePrice = t.Pricing.Any() ? t.Pricing.First().Price : 0,
                
                // Availability
                BookedSeats = t.Bookings.Count(b => b.Status == Domain.Enums.BookingStatus.Confirmed),
                AvailableSeats = t.Bus.TotalSeats - t.Bookings.Count(b => b.Status == Domain.Enums.BookingStatus.Confirmed)
            })
            .FirstOrDefaultAsync(cancellationToken);

        if (trip == null)
        {
            throw new Exception("Trip not found");
        }

        return trip;
    }
}
