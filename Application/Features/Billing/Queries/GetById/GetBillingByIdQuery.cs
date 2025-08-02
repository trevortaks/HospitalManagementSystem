using HospitalManagementSystem.Application.Common.Models;
using HospitalManagementSystem.Application.DTOs;

namespace HospitalManagementSystem.Application.Features.Billing.Queries.GetById;

public class GetBillingByIdQuery : BaseQuery<BillingDto>
{
    public int BillingId { get; set; }
}
