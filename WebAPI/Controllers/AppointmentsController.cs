using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using System.Threading.Tasks;
using System.Collections.Generic;
using MediatR;
using HospitalManagementSystem.Application.Features.Appointments.Commands;
using HospitalManagementSystem.Application.Features.Appointments.Queries;
using HospitalManagementSystem.Application.DTOs;
using HospitalManagementSystem.Application.Features.Appointments.Commands.Create;
using HospitalManagementSystem.Application.Features.Appointments.Queries.GetAll;
using HospitalManagementSystem.Application.Features.Appointments.Queries.GetById;

namespace HospitalManagementSystem.WebAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class AppointmentsController : ControllerBase
    {
        private readonly IMediator _mediator;

        public AppointmentsController(IMediator mediator)
        {
            _mediator = mediator;
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<AppointmentDto>>> GetAppointments()
        {
            var appointments = await _mediator.Send(new GetAllAppointmentsQuery());
            return Ok(appointments);
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<AppointmentDto>> GetAppointment(int id)
        {
            var appointment = await _mediator.Send(new GetAppointmentByIdQuery { AppointmentId = id });
            if (appointment == null)
            {
                return NotFound();
            }
            return Ok(appointment);
        }

        [HttpPost]
        public async Task<ActionResult<int>> CreateAppointment([FromBody] CreateAppointmentCommand command)
        {
            var appointmentId = await _mediator.Send(command);
            return CreatedAtAction(nameof(GetAppointment), new { id = appointmentId }, appointmentId);
        }
    }
}
