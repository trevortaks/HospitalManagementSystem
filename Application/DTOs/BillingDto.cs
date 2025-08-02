namespace HospitalManagementSystem.Application.DTOs;

public class BillingDto
{
    public int Id { get; set; }
    public int PatientId { get; set; }
    public DateTime BillingDate { get; set; }
    public decimal TotalAmount { get; set; }
    public List<BillingItemDto> Items { get; set; } = new();
}
