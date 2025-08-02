using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace HospitalManagementSystem.Domain.Entities;

public class Patient : BaseEntity
{
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public int PatientId { get; set; }

    [Required]
    [StringLength(50)]
    public string FirstName { get; set; }

    [Required]
    [StringLength(50)]
    public string LastName { get; set; }

    [Required]
    [DataType(DataType.Date)]
    public DateTime DateOfBirth { get; set; }

    [Required]
    [StringLength(1)]
    public string Gender { get; set; }

    [StringLength(5)]
    public string BloodGroup { get; set; }

    [Required]
    [StringLength(20)]
    [Phone]
    public string ContactNumber { get; set; }

    [StringLength(100)]
    [EmailAddress]
    public string Email { get; set; }

    [Required]
    [StringLength(200)]
    public string Address { get; set; }

    [StringLength(100)]
    public string EmergencyContact { get; set; }

    [Column(TypeName = "nvarchar(max)")]
    public string MedicalHistory { get; set; }

    public bool IsActive { get; set; } = true;

    // Navigation properties
    public virtual ICollection<MedicalRecord> MedicalRecords { get; set; } = new List<MedicalRecord>();
    public virtual ICollection<Appointment> Appointments { get; set; } = new List<Appointment>();
    public virtual ICollection<Billing> Bills { get; set; } = new List<Billing>();
    public virtual ICollection<Prescription> Prescriptions { get; set; } = new List<Prescription>();
    public virtual ICollection<RoomAssignment> RoomAssignments { get; set; } = new List<RoomAssignment>();
}