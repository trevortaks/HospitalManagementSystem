namespace HospitalMS.Data.Persistence.Entities;

public sealed class Department
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public Guid? HeadUserId { get; set; }
    public bool IsActive { get; set; } = true;
    public DateTime CreatedAtUtc { get; set; } = DateTime.UtcNow;

    public User? HeadUser { get; set; }
    public ICollection<EmployeeRecord> Employees { get; set; } = [];
}
