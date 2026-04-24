using BusBooking.Domain.Enums;

namespace BusBooking.Application.Admin.DTOs;

public class OperatorRequestDto
{
    public Guid Id { get; set; }
    public string FullName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string Phone { get; set; } = string.Empty;
    public string CompanyName { get; set; } = string.Empty;
    public string LicenseNumber { get; set; } = string.Empty;
    public string Address { get; set; } = string.Empty;
    public OperatorStatus Status { get; set; }
    public DateTime CreatedAt { get; set; }
}
