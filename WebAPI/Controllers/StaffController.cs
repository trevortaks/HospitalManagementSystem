using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using System.Threading.Tasks;
using System.Collections.Generic;
using MediatR;
using HospitalManagementSystem.Application.Features.Staff.Commands;
using HospitalManagementSystem.Application.Features.Staff.Queries;
using HospitalManagementSystem.Application.DTOs;
using HospitalManagementSystem.Application.Features.Staff.Commands.Create;
using HospitalManagementSystem.Application.Features.Staff.Commands.Delete;
using HospitalManagementSystem.Application.Features.Staff.Commands.Update;
using HospitalManagementSystem.Application.Features.Staff.Queries.GetAll;
using HospitalManagementSystem.Application.Features.Staff.Queries.GetById;

namespace HospitalManagementSystem.WebAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class StaffController : ControllerBase
    {
        private readonly IMediator _mediator;

        public StaffController(IMediator mediator)
        {
            _mediator = mediator;
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<StaffDto>>> GetStaff()
        {
            var staff = await _mediator.Send(new GetAllStaffQuery());
            return Ok(staff);
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<StaffDto>> GetStaffMember(int id)
        {
            var staffMember = await _mediator.Send(new GetStaffByIdQuery { StaffId = id });
            if (staffMember == null)
            {
                return NotFound();
            }
            return Ok(staffMember);
        }

        [HttpPost]
        public async Task<ActionResult<int>> CreateStaff([FromBody] CreateStaffCommand command)
        {
            var staffId = await _mediator.Send(command);
            return CreatedAtAction(nameof(GetStaffMember), new { id = staffId }, staffId);
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateStaff(int id, [FromBody] UpdateStaffCommand command)
        {
            if (id != command.StaffId)
            {
                return BadRequest();
            }

            await _mediator.Send(command);
            return NoContent();
        }

        [HttpDelete("{id}")]
        [Authorize(Roles = "Administrator")]
        public async Task<IActionResult> DeleteStaff(int id)
        {
            await _mediator.Send(new DeleteStaffCommand { StaffId = id });
            return NoContent();
        }
    }
}
