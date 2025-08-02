namespace HospitalManagementSystem.Application.DTOs;

public class PaymentDto
{
    public int Id { get; set; }
    public int BillingId { get; set; }
    public DateTime PaymentDate { get; set; }
    public decimal AmountPaid { get; set; }
    public string PaymentMethod { get; set; } // e.g., Cash, Card, Insurance
    public string TransactionReference { get; set; }
}
