using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace HospitalManagementSystem.Domain.Entities;

public class RoomType : BaseEntity
{
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public int RoomTypeId { get; set; }

    [Required]
    [StringLength(100)]
    public string Name { get; set; }

    [StringLength(500)]
    public string Description { get; set; }

    [Required]
    [Column(TypeName = "decimal(18,2)")]
    public decimal BasePricePerDay { get; set; }

    [Required]
    public int Capacity { get; set; }

    [StringLength(500)]
    public string Amenities { get; set; }

    public bool RequiresSpecialEquipment { get; set; }

    [StringLength(500)]
    public string EquipmentRequirements { get; set; }

    public bool IsForEmergency { get; set; }

    public bool IsForSurgery { get; set; }

    public bool IsForICU { get; set; }

    public bool IsForMaternity { get; set; }

    public bool IsForPediatrics { get; set; }

    [StringLength(100)]
    public string VisitingHours { get; set; }

    [StringLength(1000)]
    public string Rules { get; set; }

    public bool IsActive { get; set; } = true;

    // Navigation properties
    public virtual ICollection<Room> Rooms { get; set; } = new List<Room>();
}