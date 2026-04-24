using BusBooking.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;

namespace BusBooking.Application.Common.Interfaces;

public interface IAppDbContext
{
    DbSet<User> Users { get; }
    DbSet<BusOperator> BusOperators { get; }
    DbSet<RefreshToken> RefreshTokens { get; }
    DbSet<Station> Stations { get; }
    DbSet<BusRoute> Routes { get; }
    DbSet<Bus> Buses { get; }
    DbSet<Trip> Trips { get; }
    DbSet<TripPricing> TripPricings { get; }
    DbSet<Booking> Bookings { get; }
    DbSet<BookingSeat> BookingSeats { get; }
    DbSet<SeatLayout> SeatLayouts { get; }
    DbSet<Notification> Notifications { get; }
    
    DatabaseFacade Database { get; }
    
    Task<int> SaveChangesAsync(CancellationToken cancellationToken);
}
