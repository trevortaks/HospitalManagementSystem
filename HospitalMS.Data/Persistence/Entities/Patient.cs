namespace HospitalMS.Data.Persistence.Entities;

public sealed class Patient
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public string MedicalRecordNumber { get; set; } = string.Empty;
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public DateTime DateOfBirth { get; set; }
    public string Email { get; set; } = string.Empty;
    public string? PhoneNumber { get; set; }
    public string? Gender { get; set; }
    public string? BloodGroup { get; set; }
    public string? AddressLine1 { get; set; }
    public string? City { get; set; }
    public string? PostalCode { get; set; }
    public string? Country { get; set; }
    public string? AddressLine2 { get; set; }
    public string? State { get; set; }
    public string? EmergencyContactName { get; set; }
    public string? EmergencyContactPhone { get; set; }
    public string? EmergencyContactRelationship { get; set; }
    public string? EmergencyContactEmail { get; set; }
    public string? Allergies { get; set; }
    public string? ChronicConditions { get; set; }
    public string? Notes { get; set; }
    public DateTime CreatedAtUtc { get; set; } = DateTime.UtcNow;
    public DateTime? UpdatedAtUtc { get; set; }
}
