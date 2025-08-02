using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using System.Threading.Tasks;
using System.Collections.Generic;
using MediatR;
using HospitalManagementSystem.Application.Features.Patients.Commands;
using HospitalManagementSystem.Application.Features.Patients.Queries;
using HospitalManagementSystem.Application.DTOs;
using HospitalManagementSystem.Application.Features.Patients.Commands.Create;
using HospitalManagementSystem.Application.Features.Patients.Commands.Delete;
using HospitalManagementSystem.Application.Features.Patients.Commands.Update;
using HospitalManagementSystem.Application.Features.Patients.Queries.GetAll;
using HospitalManagementSystem.Application.Features.Patients.Queries.GetById;

namespace HospitalManagementSystem.WebAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class PatientsController : ControllerBase
    {
        private readonly IMediator _mediator;

        public PatientsController(IMediator mediator)
        {
            _mediator = mediator;
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<PatientDto>>> GetPatients([FromQuery] bool includeInactive = false)
        {
            var query = new GetAllPatientsQuery { IncludeInactive = includeInactive };
            var patients = await _mediator.Send(query);
            return Ok(patients);
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<PatientDto>> GetPatient(int id)
        {
            var query = new GetPatientByIdQuery { Id = id };
            var patient = await _mediator.Send(query);
            
            if (patient == null)
            {
                return NotFound();
            }

            return Ok(patient);
        }

        [HttpPost]
        public async Task<ActionResult<int>> CreatePatient([FromBody] CreatePatientCommand command)
        {
            var patientId = await _mediator.Send(command);
            return CreatedAtAction(nameof(GetPatient), new { id = patientId }, patientId);
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> UpdatePatient(int id, [FromBody] UpdatePatientCommand command)
        {
            if (id != command.PatientId)
            {
                return BadRequest();
            }

            await _mediator.Send(command);
            return NoContent();
        }

        [HttpDelete("{id}")]
        [Authorize(Roles = "Administrator,Doctor")]
        public async Task<IActionResult> DeletePatient(int id)
        {
            var command = new DeletePatientCommand { PatientId = id };
            await _mediator.Send(command);
            return NoContent();
        }
    }
}
