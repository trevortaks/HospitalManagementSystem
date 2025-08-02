using System;
using HospitalManagementSystem.Application.Common.Models;
using HospitalManagementSystem.Application.DTOs;

namespace HospitalManagementSystem.Application.Features.Billing.Commands.ProcessPayment;

public class ProcessPaymentCommand : BaseCommand<PaymentResultDto>
{
    public int BillId { get; set; }
    public decimal Amount { get; set; }
    public string PaymentMethod { get; set; }
    public string TransactionId { get; set; }
    public string ProcessedBy { get; set; }
}