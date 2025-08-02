using System.ComponentModel.DataAnnotations;

namespace HospitalManagementSystem.Domain.Entities;

public abstract class BaseEntity
{
    [Required]
    public DateTime CreatedAt { get; set; }

    [StringLength(100)]
    public string CreatedBy { get; set; }

    public DateTime? UpdatedAt { get; set; }

    [StringLength(100)]
    public string UpdatedBy { get; set; }
}