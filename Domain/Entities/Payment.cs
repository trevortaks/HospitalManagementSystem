using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace HospitalManagementSystem.Domain.Entities;

public class Payment
{
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public int PaymentId { get; set; }

    [Required]
    public int BillId { get; set; }

    [ForeignKey("BillId")]
    public virtual Billing Bill { get; set; }

    [Required]
    [Column(TypeName = "decimal(18,2)")]
    public decimal Amount { get; set; }

    [Required]
    [StringLength(50)]
    public string PaymentMethod { get; set; }

    [StringLength(100)]
    public string TransactionId { get; set; }

    [Required]
    [DataType(DataType.DateTime)]
    public DateTime PaymentDate { get; set; }

    [StringLength(20)]
    public string Status { get; set; } = "Completed";

    [Column(TypeName = "nvarchar(max)")]
    public string Notes { get; set; }

    [StringLength(100)]
    public string ProcessedBy { get; set; }

    [Required]
    [DataType(DataType.DateTime)]
    public DateTime CreatedAt { get; set; }
}