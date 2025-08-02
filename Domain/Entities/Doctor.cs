using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace HospitalManagementSystem.Domain.Entities;

public class Doctor : BaseEntity
{
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public int DoctorId { get; set; }

    [Required]
    [StringLength(50)]
    public string FirstName { get; set; }

    [Required]
    [StringLength(50)]
    public string LastName { get; set; }

    [Required]
    [StringLength(100)]
    public string Specialization { get; set; }

    [Required]
    [StringLength(50)]
    public string LicenseNumber { get; set; }

    [StringLength(20)]
    [Phone]
    public string ContactNumber { get; set; }

    [StringLength(100)]
    [EmailAddress]
    public string Email { get; set; }

    [Required]
    public int DepartmentId { get; set; }

    [ForeignKey("DepartmentId")]
    public virtual Department Department { get; set; }

    [StringLength(20)]
    public string Status { get; set; } = "Active";

    // Foreign key for self-reference (Head Doctor)
    public int? HeadDoctorId { get; set; }

    [ForeignKey("HeadDoctorId")]
    public virtual Doctor HeadDoctor { get; set; }

    // Navigation properties
    public virtual ICollection<Doctor> SubordinateDoctors { get; set; } = new List<Doctor>();
    public virtual ICollection<MedicalRecord> MedicalRecords { get; set; } = new List<MedicalRecord>();
    public virtual ICollection<Appointment> Appointments { get; set; } = new List<Appointment>();
    public virtual ICollection<Prescription> Prescriptions { get; set; } = new List<Prescription>();
}