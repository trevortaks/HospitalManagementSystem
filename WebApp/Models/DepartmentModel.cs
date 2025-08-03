namespace HospitalManagementSystem.WebApp.Models;

public class DepartmentModel
{
    public int DepartmentId { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string Location { get; set; } = string.Empty;
    public int? HeadDoctorId { get; set; }
    public bool IsActive { get; set; }
}
