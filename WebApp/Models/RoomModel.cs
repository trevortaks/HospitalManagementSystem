namespace HospitalManagementSystem.WebApp.Models;

public class RoomModel
{
    public int RoomId { get; set; }
    public string RoomNumber { get; set; } = string.Empty;
    public int RoomTypeId { get; set; }
    public string Status { get; set; } = string.Empty;
    public int Capacity { get; set; }
    public int DepartmentId { get; set; }
    public int FloorNumber { get; set; }
}
