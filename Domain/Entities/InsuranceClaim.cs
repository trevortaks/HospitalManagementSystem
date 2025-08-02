using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace HospitalManagementSystem.Domain.Entities;

public class InsuranceClaim
{
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public int ClaimId { get; set; }

    [Required]
    public int BillId { get; set; }

    [ForeignKey("BillId")]
    public virtual Billing Bill { get; set; }

    [Required]
    [StringLength(100)]
    public string ClaimNumber { get; set; }

    [Required]
    [Column(TypeName = "decimal(18,2)")]
    public decimal ClaimAmount { get; set; }

    [Required]
    [DataType(DataType.DateTime)]
    public DateTime SubmittedDate { get; set; }

    [StringLength(20)]
    public string Status { get; set; } = "Submitted";

    [DataType(DataType.DateTime)]
    public DateTime? ApprovedDate { get; set; }

    [Column(TypeName = "decimal(18,2)")]
    public decimal? ApprovedAmount { get; set; }

    [StringLength(500)]
    public string RejectionReason { get; set; }

    [Column(TypeName = "nvarchar(max)")]
    public string Notes { get; set; }

    [StringLength(100)]
    public string SubmittedBy { get; set; }

    [Required]
    [DataType(DataType.DateTime)]
    public DateTime CreatedAt { get; set; }
}