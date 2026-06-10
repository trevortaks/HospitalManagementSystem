namespace HospitalMS.Data.Persistence.Entities;

public sealed class User
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public string Username { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string Role { get; set; } = string.Empty;
    public string PasswordHash { get; set; } = string.Empty;
    public bool IsActive { get; set; } = true;
    public DateTime CreatedAtUtc { get; set; } = DateTime.UtcNow;

    public string? FirstName { get; set; }
    public string? LastName { get; set; }
    public string? PhoneNumber { get; set; }
    public string? AddressLine1 { get; set; }
    public string? City { get; set; }
    public string? PostalCode { get; set; }
    public string? Country { get; set; }
    public string? Specialization { get; set; }
    public string? LicenseNumber { get; set; }
    public string? Bio { get; set; }

    // Phase 3 — links a Patient-role user to their patient record
    public Guid? LinkedPatientId { get; set; }
    public Patient? LinkedPatient { get; set; }
}
