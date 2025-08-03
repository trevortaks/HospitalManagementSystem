namespace HospitalManagementSystem.WebApp.Models;

public class StaffModel
{
    public int StaffId { get; set; }
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public string Position { get; set; } = string.Empty;
    public int DepartmentId { get; set; }
    public string ContactNumber { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
}
