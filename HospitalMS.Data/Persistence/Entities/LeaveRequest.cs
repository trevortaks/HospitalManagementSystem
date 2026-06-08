namespace HospitalMS.Data.Persistence.Entities;

public sealed class LeaveRequest
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public Guid EmployeeRecordId { get; set; }
    public string LeaveType { get; set; } = "Annual";
    public DateTime StartDate { get; set; }
    public DateTime EndDate { get; set; }
    public string? Reason { get; set; }
    public string Status { get; set; } = LeaveRequestStatus.Pending;
    public Guid? ReviewedByUserId { get; set; }
    public string? ReviewNotes { get; set; }
    public DateTime RequestedAtUtc { get; set; } = DateTime.UtcNow;
    public DateTime? ReviewedAtUtc { get; set; }

    public EmployeeRecord EmployeeRecord { get; set; } = null!;
    public User? ReviewedByUser { get; set; }
}

public static class LeaveType
{
    public const string Annual   = "Annual";
    public const string Sick     = "Sick";
    public const string Maternity = "Maternity";
    public const string Paternity = "Paternity";
    public const string Unpaid   = "Unpaid";
    public const string Emergency = "Emergency";
    public static readonly string[] All = [Annual, Sick, Maternity, Paternity, Unpaid, Emergency];
}

public static class LeaveRequestStatus
{
    public const string Pending  = "Pending";
    public const string Approved = "Approved";
    public const string Rejected = "Rejected";
    public const string Cancelled = "Cancelled";
}
