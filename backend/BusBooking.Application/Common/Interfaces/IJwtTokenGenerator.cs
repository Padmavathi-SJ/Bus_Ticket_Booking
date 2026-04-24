using BusBooking.Domain.Entities;

namespace BusBooking.Application.Common.Interfaces;

public interface IJwtTokenGenerator
{
    string GenerateToken(User user);
}
