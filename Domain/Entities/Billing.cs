using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace HospitalManagementSystem.Domain.Entities;

public class Billing : BaseEntity
{
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public int BillId { get; set; }

    [Required]
    public int PatientId { get; set; }

    [ForeignKey("PatientId")]
    public virtual Patient Patient { get; set; }

    public int? AppointmentId { get; set; }

    [ForeignKey("AppointmentId")]
    public virtual Appointment Appointment { get; set; }

    public int? MedicalRecordId { get; set; }

    [ForeignKey("MedicalRecordId")]
    public virtual MedicalRecord MedicalRecord { get; set; }

    [Required]
    [DataType(DataType.DateTime)]
    public DateTime BillDate { get; set; }

    [DataType(DataType.DateTime)]
    public DateTime? DueDate { get; set; }

    [Required]
    [Column(TypeName = "decimal(18,2)")]
    public decimal TotalAmount { get; set; }

    [Column(TypeName = "decimal(18,2)")]
    public decimal InsuranceAmount { get; set; }

    [Required]
    [Column(TypeName = "decimal(18,2)")]
    public decimal PatientAmount { get; set; }

    [Column(TypeName = "decimal(18,2)")]
    public decimal AmountPaid { get; set; }

    [Column(TypeName = "decimal(18,2)")]
    public decimal BalanceAmount { get; set; }

    [Required]
    [StringLength(20)]
    public string PaymentStatus { get; set; } = "Unpaid";

    [Required]
    [StringLength(50)]
    public string BillingType { get; set; }

    [StringLength(100)]
    public string InsuranceProvider { get; set; }

    [StringLength(100)]
    public string PolicyNumber { get; set; }

    [StringLength(100)]
    public string ClaimNumber { get; set; }

    [DataType(DataType.DateTime)]
    public DateTime? ClaimSubmittedDate { get; set; }

    [DataType(DataType.DateTime)]
    public DateTime? ClaimApprovedDate { get; set; }

    [Column(TypeName = "decimal(18,2)")]
    public decimal? ClaimApprovedAmount { get; set; }

    [StringLength(20)]
    public string ClaimStatus { get; set; } = "NotApplicable";

    [StringLength(50)]
    public string PaymentMethod { get; set; }

    [StringLength(100)]
    public string TransactionId { get; set; }

    [DataType(DataType.DateTime)]
    public DateTime? LastPaymentDate { get; set; }

    [Column(TypeName = "nvarchar(max)")]
    public string Notes { get; set; }

    [Required]
    [StringLength(20)]
    public string Status { get; set; } = "Active";

    // Navigation properties
    public virtual ICollection<BillingItem> BillingItems { get; set; } = new List<BillingItem>();
    public virtual ICollection<Payment> Payments { get; set; } = new List<Payment>();
    public virtual ICollection<InsuranceClaim> InsuranceClaims { get; set; } = new List<InsuranceClaim>();
}