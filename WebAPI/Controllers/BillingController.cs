using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using System.Threading.Tasks;
using System.Collections.Generic;
using MediatR;
using HospitalManagementSystem.Application.Features.Billing.Commands;
using HospitalManagementSystem.Application.Features.Billing.Queries;
using HospitalManagementSystem.Application.DTOs;

namespace HospitalManagementSystem.WebAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class BillingController : ControllerBase
    {
        private readonly IMediator _mediator;

        public BillingController(IMediator mediator)
        {
            _mediator = mediator;
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<BillingDto>>> GetBillings()
        {
            var billings = await _mediator.Send(new GetAllBillingsQuery());
            return Ok(billings);
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<BillingDto>> GetBilling(int id)
        {
            var billing = await _mediator.Send(new GetBillingByIdQuery { BillingId = id });
            if (billing == null)
            {
                return NotFound();
            }
            return Ok(billing);
        }

        [HttpPost]
        public async Task<ActionResult<PaymentResultDto>> ProcessPayment([FromBody] ProcessPaymentCommand command)
        {
            var result = await _mediator.Send(command);
            return Ok(result);
        }
    }
}
