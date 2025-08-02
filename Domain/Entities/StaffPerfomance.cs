using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace HospitalManagementSystem.Domain.Entities;

public class StaffPerformance
{
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public int PerformanceId { get; set; }

    [Required]
    public int StaffId { get; set; }

    [ForeignKey("StaffId")]
    public virtual Staff Staff { get; set; }

    [Required]
    [DataType(DataType.DateTime)]
    public DateTime ReviewDate { get; set; }

    [Required]
    [Column(TypeName = "decimal(3,2)")]
    public decimal Rating { get; set; }

    [Required]
    [StringLength(100)]
    public string Reviewer { get; set; }

    [Required]
    [StringLength(50)]
    public string ReviewType { get; set; }

    [Required]
    [StringLength(1000)]
    public string Strengths { get; set; }

    [Required]
    [StringLength(1000)]
    public string AreasForImprovement { get; set; }

    [Required]
    [StringLength(1000)]
    public string Goals { get; set; }

    [Column(TypeName = "nvarchar(max)")]
    public string Comments { get; set; }

    [StringLength(20)]
    public string Status { get; set; } = "Completed";

    [DataType(DataType.DateTime)]
    public DateTime? NextReviewDate { get; set; }

    [StringLength(100)]
    public string CreatedBy { get; set; }

    [Required]
    [DataType(DataType.DateTime)]
    public DateTime CreatedAt { get; set; }
}