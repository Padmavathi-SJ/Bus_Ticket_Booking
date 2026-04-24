using BusBooking.Application.Auth.DTOs;
using BusBooking.Application.Common.Interfaces;
using BusBooking.Domain.Enums;
using MediatR;
using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;

namespace BusBooking.Application.Auth.Commands.Login;

public class LoginCommand : IRequest<AuthResponseDto>
{
    [Required]
    [EmailAddress]
    public string Email { get; set; } = string.Empty;

    [Required]
    public string Password { get; set; } = string.Empty;

    [Required]
    public BusBooking.Domain.Enums.UserRole Role { get; set; }
}

public class LoginCommandHandler : IRequestHandler<LoginCommand, AuthResponseDto>
{
    private readonly IAppDbContext _context;
    private readonly IJwtTokenGenerator _jwtTokenGenerator;

    public LoginCommandHandler(IAppDbContext context, IJwtTokenGenerator jwtTokenGenerator)
    {
        _context = context;
        _jwtTokenGenerator = jwtTokenGenerator;
    }

    public async Task<AuthResponseDto> Handle(LoginCommand request, CancellationToken cancellationToken)
    {
        var user = await _context.Users
            .FirstOrDefaultAsync(u => u.Email == request.Email, cancellationToken);

        if (user == null)
            throw new UnauthorizedAccessException("Invalid credentials.");

        if (!user.IsActive)
        {
            if (user.Role == UserRole.BusOperator)
            {
                var busOperator = await _context.BusOperators.FirstOrDefaultAsync(o => o.UserId == user.Id, cancellationToken);
                if (busOperator?.Status == OperatorStatus.Pending)
                    throw new UnauthorizedAccessException("Your operator account is pending admin approval.");
                if (busOperator?.Status == OperatorStatus.Rejected)
                    throw new UnauthorizedAccessException("Your operator account has been rejected.");
            }
            throw new UnauthorizedAccessException("Your account is inactive. Please contact support.");
        }

        if (user.Role != request.Role)
            throw new UnauthorizedAccessException("Invalid credentials for this portal.");

        // We allow plain text login for the Master Admin for development ease,
        // otherwise we verify using BCrypt.
        bool isPasswordValid = false;
        
        if (user.Role == Domain.Enums.UserRole.Admin && user.PasswordHash == request.Password)
        {
            isPasswordValid = true;
        }
        else if (BCrypt.Net.BCrypt.Verify(request.Password, user.PasswordHash))
        {
            isPasswordValid = true;
        }

        if (!isPasswordValid)
            throw new UnauthorizedAccessException("Invalid credentials.");

        var token = _jwtTokenGenerator.GenerateToken(user);

        return new AuthResponseDto
        {
            Token = token,
            RefreshToken = "mock-refresh-token", // Implementing refresh tokens is an advanced feature
            User = new UserDto
            {
                Id = user.Id,
                Email = user.Email,
                FullName = user.FullName,
                Role = user.Role,
                IsActive = user.IsActive
            }
        };
    }
}
