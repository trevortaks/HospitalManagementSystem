using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace HospitalManagementSystem.Domain.Entities;

public class Staff : BaseEntity
{
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public int StaffId { get; set; }

    [Required]
    [StringLength(50)]
    public string FirstName { get; set; }

    [Required]
    [StringLength(50)]
    public string LastName { get; set; }

    [Required]
    [StringLength(100)]
    public string Position { get; set; }

    [StringLength(100)]
    public string Specialization { get; set; }

    [Required]
    [StringLength(50)]
    public string EmployeeId { get; set; }

    [Required]
    public int DepartmentId { get; set; }

    [ForeignKey("DepartmentId")]
    public virtual Department Department { get; set; }

    [Required]
    [StringLength(20)]
    [Phone]
    public string ContactNumber { get; set; }

    [StringLength(100)]
    [EmailAddress]
    public string Email { get; set; }

    [Required]
    [StringLength(200)]
    public string Address { get; set; }

    [Required]
    [DataType(DataType.Date)]
    public DateTime DateOfBirth { get; set; }

    [Required]
    [DataType(DataType.Date)]
    public DateTime HireDate { get; set; }

    [Required]
    [StringLength(20)]
    public string Status { get; set; } = "Active";

    [Required]
    [StringLength(20)]
    public string EmploymentType { get; set; }

    [Required]
    [Column(TypeName = "decimal(18,2)")]
    public decimal Salary { get; set; }

    [Required]
    [StringLength(20)]
    public string Shift { get; set; }

    [Required]
    [StringLength(100)]
    public string WorkingHours { get; set; }

    public bool IsOnDuty { get; set; }

    [DataType(DataType.DateTime)]
    public DateTime? LastLogin { get; set; }

    [StringLength(50)]
    public string LicenseNumber { get; set; }

    [DataType(DataType.Date)]
    public DateTime? LicenseExpiryDate { get; set; }

    [StringLength(100)]
    public string Certification { get; set; }

    [DataType(DataType.Date)]
    public DateTime? CertificationExpiryDate { get; set; }

    [StringLength(100)]
    public string EmergencyContact { get; set; }

    [StringLength(20)]
    public string EmergencyContactPhone { get; set; }

    [StringLength(5)]
    public string BloodGroup { get; set; }

    [Column(TypeName = "nvarchar(max)")]
    public string Notes { get; set; }

    // Navigation properties
    public virtual ICollection<RoomAssignment> RoomAssignments { get; set; } = new List<RoomAssignment>();
    public virtual ICollection<StaffSchedule> Schedules { get; set; } = new List<StaffSchedule>();
    public virtual ICollection<StaffLeave> Leaves { get; set; } = new List<StaffLeave>();
    public virtual ICollection<StaffPerformance> PerformanceReviews { get; set; } = new List<StaffPerformance>();
}