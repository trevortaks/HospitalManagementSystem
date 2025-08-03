using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using System.Threading.Tasks;
using System.Collections.Generic;
using MediatR;
using HospitalManagementSystem.Application.Features.Doctors.Commands;
using HospitalManagementSystem.Application.Features.Doctors.Queries;
using HospitalManagementSystem.Application.DTOs;
using HospitalManagementSystem.Application.Features.Doctors.Commands.Create;
using HospitalManagementSystem.Application.Features.Doctors.Commands.Delete;
using HospitalManagementSystem.Application.Features.Doctors.Commands.Update;
using HospitalManagementSystem.Application.Features.Doctors.Queries.GetAll;
using HospitalManagementSystem.Application.Features.Doctors.Queries.GetById;

namespace HospitalManagementSystem.WebAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class DoctorsController : ControllerBase
    {
        private readonly IMediator _mediator;

        public DoctorsController(IMediator mediator)
        {
            _mediator = mediator;
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<DoctorDto>>> GetDoctors()
        {
            var doctors = await _mediator.Send(new GetAllDoctorsQuery());
            return Ok(doctors);
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<DoctorDto>> GetDoctor(int id)
        {
            var doctor = await _mediator.Send(new GetDoctorByIdQuery { DoctorId = id });
            if (doctor == null)
            {
                return NotFound();
            }
            return Ok(doctor);
        }

        [HttpPost]
        public async Task<ActionResult<int>> CreateDoctor([FromBody] CreateDoctorCommand command)
        {
            var doctorId = await _mediator.Send(command);
            return CreatedAtAction(nameof(GetDoctor), new { id = doctorId }, doctorId);
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateDoctor(int id, [FromBody] UpdateDoctorCommand command)
        {
            if (id != command.DoctorId)
            {
                return BadRequest();
            }

            await _mediator.Send(command);
            return NoContent();
        }

        [HttpDelete("{id}")]
        [Authorize(Roles = "Administrator")]
        public async Task<IActionResult> DeleteDoctor(int id)
        {
            await _mediator.Send(new DeleteDoctorCommand { DoctorId = id });
            return NoContent();
        }
    }
}
