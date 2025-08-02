using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using System.Threading.Tasks;
using System.Collections.Generic;
using MediatR;
using HospitalManagementSystem.Application.Features.MedicalRecords.Commands;
using HospitalManagementSystem.Application.Features.MedicalRecords.Queries;
using HospitalManagementSystem.Application.DTOs;

namespace HospitalManagementSystem.WebAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class MedicalRecordsController : ControllerBase
    {
        private readonly IMediator _mediator;

        public MedicalRecordsController(IMediator mediator)
        {
            _mediator = mediator;
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<MedicalRecordDto>>> GetMedicalRecords()
        {
            var records = await _mediator.Send(new GetAllMedicalRecordsQuery());
            return Ok(records);
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<MedicalRecordDto>> GetMedicalRecord(int id)
        {
            var record = await _mediator.Send(new GetMedicalRecordByIdQuery { RecordId = id });
            if (record == null)
            {
                return NotFound();
            }
            return Ok(record);
        }

        [HttpPost]
        public async Task<ActionResult<int>> CreateMedicalRecord([FromBody] CreateMedicalRecordCommand command)
        {
            var recordId = await _mediator.Send(command);
            return CreatedAtAction(nameof(GetMedicalRecord), new { id = recordId }, recordId);
        }
    }
}
