using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace HospitalManagementSystem.Domain.Entities;

public class MedicalRecord : BaseEntity
{
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public int RecordId { get; set; }

    [Required]
    public int PatientId { get; set; }

    [ForeignKey("PatientId")]
    public virtual Patient Patient { get; set; }

    [Required]
    public int DoctorId { get; set; }

    [ForeignKey("DoctorId")]
    public virtual Doctor Doctor { get; set; }

    [Required]
    [DataType(DataType.DateTime)]
    public DateTime VisitDate { get; set; }

    [Required]
    [Column(TypeName = "nvarchar(max)")]
    public string Diagnosis { get; set; }

    [Column(TypeName = "nvarchar(max)")]
    public string Treatment { get; set; }

    [Column(TypeName = "nvarchar(max)")]
    public string Notes { get; set; }

    [DataType(DataType.DateTime)]
    public DateTime? FollowUpDate { get; set; }

    // Navigation properties
    public virtual ICollection<Prescription> Prescriptions { get; set; } = new List<Prescription>();
    public virtual ICollection<Billing> Bills { get; set; } = new List<Billing>();
}