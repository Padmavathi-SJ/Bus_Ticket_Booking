using BusBooking.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace BusBooking.Infrastructure.Persistence.Configurations;

public class BusRouteConfiguration : IEntityTypeConfiguration<BusRoute>
{
    public void Configure(EntityTypeBuilder<BusRoute> builder)
    {
        builder.ToTable("Routes");
        builder.Property(r => r.Source).IsRequired();
        builder.Property(r => r.Destination).IsRequired();
    }
}
