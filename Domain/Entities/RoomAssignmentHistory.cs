using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace HospitalManagementSystem.Domain.Entities;

public class RoomAssignmentHistory
{
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public int HistoryId { get; set; }

    [Required]
    public int AssignmentId { get; set; }

    [ForeignKey("AssignmentId")]
    public virtual RoomAssignment Assignment { get; set; }

    [Required]
    [StringLength(50)]
    public string Action { get; set; }

    [Required]
    [StringLength(1000)]
    public string Details { get; set; }

    [Required]
    [DataType(DataType.DateTime)]
    public DateTime ActionDate { get; set; }

    [StringLength(100)]
    public string PerformedBy { get; set; }
}