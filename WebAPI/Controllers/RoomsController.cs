using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using System.Threading.Tasks;
using System.Collections.Generic;
using MediatR;
using HospitalManagementSystem.Application.Features.Rooms.Commands;
using HospitalManagementSystem.Application.Features.Rooms.Queries;
using HospitalManagementSystem.Application.DTOs;

namespace HospitalManagementSystem.WebAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class RoomsController : ControllerBase
    {
        private readonly IMediator _mediator;

        public RoomsController(IMediator mediator)
        {
            _mediator = mediator;
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<RoomDto>>> GetRooms()
        {
            var rooms = await _mediator.Send(new GetAllRoomsQuery());
            return Ok(rooms);
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<RoomDto>> GetRoom(int id)
        {
            var room = await _mediator.Send(new GetRoomByIdQuery { RoomId = id });
            if (room == null)
            {
                return NotFound();
            }
            return Ok(room);
        }

        [HttpPost]
        public async Task<ActionResult<int>> CreateRoom([FromBody] CreateRoomCommand command)
        {
            var roomId = await _mediator.Send(command);
            return CreatedAtAction(nameof(GetRoom), new { id = roomId }, roomId);
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateRoom(int id, [FromBody] UpdateRoomCommand command)
        {
            if (id != command.RoomId)
            {
                return BadRequest();
            }

            await _mediator.Send(command);
            return NoContent();
        }

        [HttpDelete("{id}")]
        [Authorize(Roles = "Administrator")]
        public async Task<IActionResult> DeleteRoom(int id)
        {
            await _mediator.Send(new DeleteRoomCommand { RoomId = id });
            return NoContent();
        }
    }
}
