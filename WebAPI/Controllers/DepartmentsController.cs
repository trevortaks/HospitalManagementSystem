using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using System.Threading.Tasks;
using System.Collections.Generic;
using MediatR;
using HospitalManagementSystem.Application.Features.Departments.Commands;
using HospitalManagementSystem.Application.Features.Departments.Queries;
using HospitalManagementSystem.Application.DTOs;
using HospitalManagementSystem.Application.Features.Departments.Commands.Create;
using HospitalManagementSystem.Application.Features.Departments.Commands.Delete;
using HospitalManagementSystem.Application.Features.Departments.Commands.Update;
using HospitalManagementSystem.Application.Features.Departments.Queries.GetAll;
using HospitalManagementSystem.Application.Features.Departments.Queries.GetById;

namespace HospitalManagementSystem.WebAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class DepartmentsController : ControllerBase
    {
        private readonly IMediator _mediator;

        public DepartmentsController(IMediator mediator)
        {
            _mediator = mediator;
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<DepartmentDto>>> GetDepartments()
        {
            var departments = await _mediator.Send(new GetAllDepartmentsQuery());
            return Ok(departments);
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<DepartmentDto>> GetDepartment(int id)
        {
            var department = await _mediator.Send(new GetDepartmentByIdQuery { DepartmentId = id });
            if (department == null)
            {
                return NotFound();
            }
            return Ok(department);
        }

        [HttpPost]
        public async Task<ActionResult<int>> CreateDepartment([FromBody] CreateDepartmentCommand command)
        {
            var departmentId = await _mediator.Send(command);
            return CreatedAtAction(nameof(GetDepartment), new { id = departmentId }, departmentId);
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateDepartment(int id, [FromBody] UpdateDepartmentCommand command)
        {
            if (id != command.DepartmentId)
            {
                return BadRequest();
            }

            await _mediator.Send(command);
            return NoContent();
        }

        [HttpDelete("{id}")]
        [Authorize(Roles = "Administrator")]
        public async Task<IActionResult> DeleteDepartment(int id)
        {
            await _mediator.Send(new DeleteDepartmentCommand { DepartmentId = id });
            return NoContent();
        }
    }
}
