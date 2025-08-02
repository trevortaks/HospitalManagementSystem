using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace HospitalManagementSystem.Domain.Entities;

public class StaffSchedule
{
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public int ScheduleId { get; set; }

    [Required]
    public int StaffId { get; set; }

    [ForeignKey("StaffId")]
    public virtual Staff Staff { get; set; }

    [Required]
    [DataType(DataType.Date)]
    public DateTime ScheduleDate { get; set; }

    [Required]
    [StringLength(50)]
    public string ShiftType { get; set; }

    [Required]
    [DataType(DataType.DateTime)]
    public DateTime StartTime { get; set; }

    [Required]
    [DataType(DataType.DateTime)]
    public DateTime EndTime { get; set; }

    [StringLength(20)]
    public string Status { get; set; } = "Scheduled";

    [StringLength(100)]
    public string Department { get; set; }

    [Column(TypeName = "nvarchar(max)")]
    public string Notes { get; set; }

    [StringLength(100)]
    public string CreatedBy { get; set; }

    [Required]
    [DataType(DataType.DateTime)]
    public DateTime CreatedAt { get; set; }
}