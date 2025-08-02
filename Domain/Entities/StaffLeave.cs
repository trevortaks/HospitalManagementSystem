using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace HospitalManagementSystem.Domain.Entities;

public class StaffLeave
{
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public int LeaveId { get; set; }

    [Required]
    public int StaffId { get; set; }

    [ForeignKey("StaffId")]
    public virtual Staff Staff { get; set; }

    [Required]
    [DataType(DataType.Date)]
    public DateTime StartDate { get; set; }

    [Required]
    [DataType(DataType.Date)]
    public DateTime EndDate { get; set; }

    [Required]
    [StringLength(50)]
    public string LeaveType { get; set; }

    [Required]
    [StringLength(500)]
    public string Reason { get; set; }

    [StringLength(20)]
    public string Status { get; set; } = "Pending";

    [Column(TypeName = "decimal(5,2)")]
    public decimal? DaysCount { get; set; }

    [StringLength(100)]
    public string ApprovedBy { get; set; }

    [DataType(DataType.DateTime)]
    public DateTime? ApprovedDate { get; set; }

    [StringLength(500)]
    public string RejectionReason { get; set; }

    [StringLength(100)]
    public string CreatedBy { get; set; }

    [Required]
    [DataType(DataType.DateTime)]
    public DateTime CreatedAt { get; set; }
}