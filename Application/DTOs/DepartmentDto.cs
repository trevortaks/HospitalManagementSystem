namespace HospitalManagementSystem.Application.DTOs;

public class DepartmentDto
{
    public int DepartmentId { get; set; }
    public string Name { get; set; }
    public string Description { get; set; }
    public string Location { get; set; }
    public int? HeadDoctorId { get; set; }
    public bool IsActive { get; set; }
}
