using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace HospitalManagementSystem.Domain.Entities;

public class Prescription : BaseEntity
{
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public int PrescriptionId { get; set; }

    [Required]
    public int PatientId { get; set; }

    [ForeignKey("PatientId")]
    public virtual Patient Patient { get; set; }

    [Required]
    public int DoctorId { get; set; }

    [ForeignKey("DoctorId")]
    public virtual Doctor Doctor { get; set; }

    [Required]
    public int MedicationId { get; set; }

    [ForeignKey("MedicationId")]
    public virtual Medication Medication { get; set; }

    [Required]
    [StringLength(100)]
    public string Dosage { get; set; }

    [Required]
    [StringLength(100)]
    public string Frequency { get; set; }

    [Required]
    [StringLength(100)]
    public string Duration { get; set; }

    [Required]
    [StringLength(500)]
    public string Instructions { get; set; }

    [Required]
    [DataType(DataType.DateTime)]
    public DateTime DatePrescribed { get; set; }

    [DataType(DataType.DateTime)]
    public DateTime? StartDate { get; set; }

    [DataType(DataType.DateTime)]
    public DateTime? EndDate { get; set; }

    [Required]
    [StringLength(20)]
    public string Status { get; set; } = "Active";

    public int? MedicalRecordId { get; set; }

    [ForeignKey("MedicalRecordId")]
    public virtual MedicalRecord MedicalRecord { get; set; }

    [DataType(DataType.DateTime)]
    public DateTime? DispensedDate { get; set; }

    [StringLength(100)]
    public string DispensedBy { get; set; }

    [Required]
    public int Quantity { get; set; }

    [Required]
    public int RefillsAllowed { get; set; }

    [Required]
    public int RefillsUsed { get; set; }
}