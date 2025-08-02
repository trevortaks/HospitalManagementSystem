using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace HospitalManagementSystem.Domain.Entities;

public class Room : BaseEntity
{
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public int RoomId { get; set; }

    [Required]
    [StringLength(20)]
    public string RoomNumber { get; set; }

    [Required]
    public int RoomTypeId { get; set; }

    [ForeignKey("RoomTypeId")]
    public virtual RoomType RoomType { get; set; }

    [Required]
    [StringLength(20)]
    public string Status { get; set; } = "Available";

    [Required]
    public int Capacity { get; set; }

    public int CurrentOccupancy { get; set; }

    public int FloorNumber { get; set; }

    [DataType(DataType.Date)]
    public DateTime? LastServiced { get; set; }

    [Required]
    public int DepartmentId { get; set; }

    [ForeignKey("DepartmentId")]
    public virtual Department Department { get; set; }

    // Navigation properties
    public virtual ICollection<Appointment> Appointments { get; set; } = new List<Appointment>();
    public virtual ICollection<RoomAssignment> Assignments { get; set; } = new List<RoomAssignment>();
}