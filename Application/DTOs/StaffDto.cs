namespace HospitalManagementSystem.Application.DTOs;

public class StaffDto
{
    public int StaffId { get; set; }
    public string FirstName { get; set; }
    public string LastName { get; set; }
    public string Position { get; set; }
    public int DepartmentId { get; set; }
    public string ContactNumber { get; set; }
    public string Email { get; set; }
    public string Status { get; set; }
}
