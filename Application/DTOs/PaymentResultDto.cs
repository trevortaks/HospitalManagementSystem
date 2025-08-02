using System;

namespace HospitalManagementSystem.Application.DTOs;

public class PaymentResultDto
{
    public int BillId { get; set; }
    public decimal AmountPaid { get; set; }
    public decimal RemainingBalance { get; set; }
    public string PaymentStatus { get; set; }
    public string TransactionId { get; set; }
    public DateTime ProcessedAt { get; set; }
}