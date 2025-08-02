using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace HospitalManagementSystem.Domain.Entities;

public class Appointment : BaseEntity
{
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public int AppointmentId { get; set; }

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
    public DateTime AppointmentDate { get; set; }

    [Required]
    [DataType(DataType.DateTime)]
    public DateTime EndTime { get; set; }

    [Required]
    [StringLength(20)]
    public string Status { get; set; } = "Scheduled";

    [Required]
    [StringLength(500)]
    public string Purpose { get; set; }

    [Column(TypeName = "nvarchar(max)")]
    public string Notes { get; set; }

    public int? RoomId { get; set; }

    [ForeignKey("RoomId")]
    public virtual Room Room { get; set; }

    // Navigation properties
    public virtual ICollection<Billing> Bills { get; set; } = new List<Billing>();
}