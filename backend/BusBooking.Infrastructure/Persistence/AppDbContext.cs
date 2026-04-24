using BusBooking.Domain.Entities;
using BusBooking.Application.Common.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace BusBooking.Infrastructure.Persistence;

public class AppDbContext : DbContext, IAppDbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

    // DbSets (Tables)
    public DbSet<User> Users => Set<User>();
    public DbSet<RefreshToken> RefreshTokens => Set<RefreshToken>();
    public DbSet<BusOperator> BusOperators => Set<BusOperator>();
    public DbSet<Station> Stations => Set<Station>();
    public DbSet<BusRoute> Routes => Set<BusRoute>();
    public DbSet<RouteStop> RouteStops => Set<RouteStop>();
    public DbSet<Bus> Buses => Set<Bus>();
    public DbSet<SeatLayout> SeatLayouts => Set<SeatLayout>();
    public DbSet<Trip> Trips => Set<Trip>();
    public DbSet<TripPricing> TripPricings => Set<TripPricing>();
    public DbSet<Booking> Bookings => Set<Booking>();
    public DbSet<BookingSeat> BookingSeats => Set<BookingSeat>();
    public DbSet<Payment> Payments => Set<Payment>();
    public DbSet<Notification> Notifications => Set<Notification>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // Apply all configurations from this assembly
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(AppDbContext).Assembly);
    }

    public override Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        // Auto-set UpdatedAt on every save
        foreach (var entry in ChangeTracker.Entries<Domain.Common.BaseEntity>())
        {
            if (entry.State == EntityState.Modified)
                entry.Entity.UpdatedAt = DateTime.UtcNow;
        }
        return base.SaveChangesAsync(cancellationToken);
    }
}
