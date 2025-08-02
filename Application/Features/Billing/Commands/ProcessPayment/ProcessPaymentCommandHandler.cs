using System.Threading;
using System.Threading.Tasks;
using MediatR;
using Microsoft.EntityFrameworkCore;
using HospitalManagementSystem.Application.Common.Interfaces;
using HospitalManagementSystem.Application.DTOs;
using HospitalManagementSystem.Domain.Entities;
using HospitalManagementSystem.Application.Common.Exceptions;

namespace HospitalManagementSystem.Application.Features.Billing.Commands.ProcessPayment;

public class ProcessPaymentCommandHandler : Common.Interfaces.IRequestHandler<ProcessPaymentCommand, PaymentResultDto>
{
    private readonly IApplicationDbContext _context;

    public ProcessPaymentCommandHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<PaymentResultDto> Handle(ProcessPaymentCommand request, CancellationToken cancellationToken)
    {
        var bill = await _context.Bills
            .Include(b => b.Payments)
            .FirstOrDefaultAsync(b => b.BillId == request.BillId && b.Status == "Active", cancellationToken);

        if (bill == null)
        {
            //throw new NotFoundException("Bill", request.BillId);
        }

        if (request.Amount <= 0)
        {
            //throw new BillingException(request.Amount, "Payment amount must be greater than zero");
        }

        if (request.Amount > bill.BalanceAmount)
        {
            //throw new BillingException(request.Amount, "Payment amount cannot exceed balance amount");
        }

        // Process payment
        bill.AmountPaid += request.Amount;
        bill.BalanceAmount -= request.Amount;
        bill.LastPaymentDate = DateTime.UtcNow;
        bill.PaymentMethod = request.PaymentMethod;
        bill.TransactionId = request.TransactionId;
        bill.UpdatedAt = DateTime.UtcNow;

        // Update payment status
        bill.PaymentStatus = bill.BalanceAmount <= 0 ? "Paid" : "PartiallyPaid";

        // Add payment record
        var payment = new Payment
        {
            BillId = bill.BillId,
            Amount = request.Amount,
            PaymentMethod = request.PaymentMethod,
            TransactionId = request.TransactionId,
            PaymentDate = DateTime.UtcNow,
            Status = "Completed",
            ProcessedBy = request.ProcessedBy,
            CreatedAt = DateTime.UtcNow
        };

        _context.Payments.Add(payment);
        await _context.SaveChangesAsync(cancellationToken);

        return new PaymentResultDto
        {
            BillId = bill.BillId,
            AmountPaid = request.Amount,
            RemainingBalance = bill.BalanceAmount,
            PaymentStatus = bill.PaymentStatus,
            TransactionId = request.TransactionId,
            ProcessedAt = DateTime.UtcNow
        };
    }
}