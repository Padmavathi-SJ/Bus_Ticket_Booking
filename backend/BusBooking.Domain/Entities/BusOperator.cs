using BusBooking.Domain.Common;
using BusBooking.Domain.Enums;

namespace BusBooking.Domain.Entities;

public class BusOperator : BaseEntity
{
    public Guid UserId { get; set; }
    public string CompanyName { get; set; } = string.Empty;
    public string LicenseNumber { get; set; } = string.Empty;
    public string ContactPhone { get; set; } = string.Empty;
    public string Address { get; set; } = string.Empty;
    public OperatorStatus Status { get; set; } = OperatorStatus.Pending;
    public Guid? ApprovedBy { get; set; }
    public DateTime? ApprovedAt { get; set; }
    public string? RejectionReason { get; set; }

    // Navigation
    public User User { get; set; } = null!;
    public ICollection<Bus> Buses { get; set; } = new List<Bus>();
}
