using BusBooking.Domain.Entities;
using BusBooking.Domain.Enums;
using BusBooking.Application.Common.Interfaces;
using MediatR;
using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;

namespace BusBooking.Application.Auth.Commands.RegisterOperator;

public class RegisterOperatorCommand : IRequest<Guid>
{
    [Required] public string FullName { get; set; } = string.Empty;
    [Required, EmailAddress] public string Email { get; set; } = string.Empty;
    [Required] public string Phone { get; set; } = string.Empty;
    [Required] public string Password { get; set; } = string.Empty;
    
    // Operator Specific Fields
    [Required] public string CompanyName { get; set; } = string.Empty;
    [Required] public string LicenseNumber { get; set; } = string.Empty;
    [Required] public string Address { get; set; } = string.Empty;
}

public class RegisterOperatorCommandHandler : IRequestHandler<RegisterOperatorCommand, Guid>
{
    private readonly IAppDbContext _context;

    public RegisterOperatorCommandHandler(IAppDbContext context)
    {
        _context = context;
    }

    public async Task<Guid> Handle(RegisterOperatorCommand request, CancellationToken cancellationToken)
    {
        if (await _context.Users.AnyAsync(u => u.Email == request.Email, cancellationToken))
            throw new Exception("Email already in use.");

        var user = new User
        {
            Id = Guid.NewGuid(),
            FullName = request.FullName,
            Email = request.Email,
            Phone = request.Phone,
            PasswordHash = BCrypt.Net.BCrypt.HashPassword(request.Password),
            Role = UserRole.BusOperator,
            IsActive = false, // User cannot login until approved by Admin
            CreatedAt = DateTime.UtcNow
        };

        var busOperator = new BusOperator
        {
            Id = Guid.NewGuid(),
            UserId = user.Id,
            CompanyName = request.CompanyName,
            LicenseNumber = request.LicenseNumber,
            Address = request.Address,
            Status = OperatorStatus.Pending, // Admin must approve this
            CreatedAt = DateTime.UtcNow
        };

        _context.Users.Add(user);
        _context.BusOperators.Add(busOperator);
        await _context.SaveChangesAsync(cancellationToken);

        return user.Id;
    }
}
