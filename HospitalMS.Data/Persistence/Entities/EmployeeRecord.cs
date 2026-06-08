namespace HospitalMS.Data.Persistence.Entities;

public sealed class EmployeeRecord
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public Guid UserId { get; set; }
    public Guid? DepartmentId { get; set; }
    public string EmployeeNumber { get; set; } = string.Empty;
    public string JobTitle { get; set; } = string.Empty;
    public string EmploymentType { get; set; } = "FullTime";
    public string Status { get; set; } = EmployeeStatus.Active;
    public DateTime HiredAtUtc { get; set; }
    public DateTime? TerminatedAtUtc { get; set; }
    public decimal Salary { get; set; }
    public string? Notes { get; set; }
    public DateTime CreatedAtUtc { get; set; } = DateTime.UtcNow;

    public User User { get; set; } = null!;
    public Department? Department { get; set; }
    public ICollection<LeaveRequest> LeaveRequests { get; set; } = [];
}

public static class EmploymentType
{
    public const string FullTime  = "FullTime";
    public const string PartTime  = "PartTime";
    public const string Contract  = "Contract";
    public const string Temporary = "Temporary";
    public static readonly string[] All = [FullTime, PartTime, Contract, Temporary];
}

public static class EmployeeStatus
{
    public const string Active     = "Active";
    public const string OnLeave    = "OnLeave";
    public const string Terminated = "Terminated";
    public const string Suspended  = "Suspended";
}
