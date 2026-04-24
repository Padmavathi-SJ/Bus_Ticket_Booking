using BusBooking.Infrastructure.Persistence;
using BusBooking.Infrastructure.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

using BusBooking.Application.Common.Interfaces;
using BusBooking.Infrastructure.Authentication;

namespace BusBooking.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        // PostgreSQL via EF Core
        services.AddDbContext<AppDbContext>(options =>
            options.UseNpgsql(configuration.GetConnectionString("DefaultConnection")));

        services.AddScoped<IAppDbContext>(provider => provider.GetRequiredService<AppDbContext>());

        services.AddScoped<IJwtTokenGenerator, JwtTokenGenerator>();
        
        // Email Service
        services.AddScoped<IEmailService, EmailService>();

        return services;
    }
}
