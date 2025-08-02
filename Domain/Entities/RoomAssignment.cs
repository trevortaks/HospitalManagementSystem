using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace HospitalManagementSystem.Domain.Entities;

public class RoomAssignment : BaseEntity
{
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public int AssignmentId { get; set; }

    [Required]
    public int RoomId { get; set; }

    [ForeignKey("RoomId")]
    public virtual Room Room { get; set; }

    public int? PatientId { get; set; }

    [ForeignKey("PatientId")]
    public virtual Patient Patient { get; set; }

    public int? StaffId { get; set; }

    [ForeignKey("StaffId")]
    public virtual Staff Staff { get; set; }

    [Required]
    [DataType(DataType.DateTime)]
    public DateTime AssignmentDate { get; set; }

    [DataType(DataType.DateTime)]
    public DateTime? ReleaseDate { get; set; }

    [Required]
    [StringLength(50)]
    public string AssignmentType { get; set; }

    [Required]
    [StringLength(20)]
    public string Status { get; set; } = "Active";

    [Required]
    [StringLength(500)]
    public string Purpose { get; set; }

    [Column(TypeName = "nvarchar(max)")]
    public string Notes { get; set; }

    [Column(TypeName = "decimal(18,2)")]
    public decimal? DailyRate { get; set; }

    [Column(TypeName = "decimal(18,2)")]
    public decimal? TotalCharges { get; set; }

    public bool IsTemporary { get; set; }

    [DataType(DataType.DateTime)]
    public DateTime? ExpectedReleaseDate { get; set; }

    [StringLength(100)]
    public string AssignedBy { get; set; }

    [StringLength(100)]
    public string ReleasedBy { get; set; }
}