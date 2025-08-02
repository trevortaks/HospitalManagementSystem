using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace HospitalManagementSystem.Domain.Entities;

public class Medication : BaseEntity
{
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public int MedicationId { get; set; }

    [Required]
    [StringLength(100)]
    public string Name { get; set; }

    [StringLength(500)]
    public string Description { get; set; }

    [Required]
    [StringLength(50)]
    public string DosageForm { get; set; }

    [Required]
    [StringLength(50)]
    public string Strength { get; set; }

    [StringLength(100)]
    public string Manufacturer { get; set; }

    [Required]
    public int StockQuantity { get; set; }

    [Required]
    public int ReorderLevel { get; set; }

    [Required]
    [Column(TypeName = "decimal(18,2)")]
    public decimal UnitPrice { get; set; }

    [DataType(DataType.Date)]
    public DateTime? ExpiryDate { get; set; }

    [StringLength(20)]
    public string Status { get; set; } = "Active";

    public bool RequiresPrescription { get; set; }

    [StringLength(200)]
    public string StorageConditions { get; set; }

    // Navigation properties
    public virtual ICollection<Prescription> Prescriptions { get; set; } = new List<Prescription>();
}