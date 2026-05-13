using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using QuickStock.Applications.Rooms.Command;
using QuickStock.Applications.Rooms.Queries;
using QuickStock.Domain.ITassets;
using QuickStock.Domain.Messaging;
using QuickStock.Domain.Social;
using QuickStock.Domain.Locations;
using QuickStock.Domain.Shared;
using QuickStock.Domain.Accounts;

namespace QuickStock.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class RoomsController : ControllerBase
    {
        private readonly IMediator _mediator;

        public RoomsController(IMediator mediator) => _mediator = mediator;

        [HttpGet]
        public async Task<ActionResult<IEnumerable<Room>>> GetRooms(int? campusId = null)
        {
            var result = await _mediator.Send(new GetRoomsQuery(campusId, User));
            return Ok(result);
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<Room>> GetRoom(int id)
        {
            var room = await _mediator.Send(new GetRoomByIdQuery(id, User));
            if (room == null) return NotFound();
            return Ok(room);
        }

        [HttpPost]
        [Authorize(Roles = "Admin,Library Admin,Home Economics Admin,Manager,User")]
        public async Task<ActionResult<Room>> CreateRoom(Room room)
        {
            try
            {
                var result = await _mediator.Send(new CreateRoomCommand(room, User));
                return CreatedAtAction(nameof(GetRoom), new { id = result.RoomId }, result);
            }
            catch (InvalidOperationException ex) { return BadRequest(ex.Message); }
        }

        [HttpPut("{id}")]
        [Authorize(Roles = "Admin,Library Admin,Home Economics Admin,Manager")]
        public async Task<IActionResult> UpdateRoom(int id, Room room)
        {
            if (id != room.RoomId) return BadRequest();
            try
            {
                await _mediator.Send(new UpdateRoomCommand(id, room, User));
                return NoContent();
            }
            catch (InvalidOperationException ex) { return BadRequest(ex.Message); }
        }

        [HttpPut("{id}/toggle-status")]
        [Authorize(Roles = "Admin,Library Admin,Home Economics Admin")]
        public async Task<IActionResult> ToggleStatus(int id)
        {
            try
            {
                var result = await _mediator.Send(new ToggleRoomStatusCommand(id, User));
                return Ok(result);
            }
            catch (KeyNotFoundException) { return NotFound(); }
        }

        [HttpDelete("{id}")]
        [Authorize(Roles = "Admin,Library Admin,Home Economics Admin")]
        public async Task<IActionResult> DeleteRoom(int id)
        {
            var success = await _mediator.Send(new DeleteRoomCommand(id, User));
            if (!success) return NotFound();
            return NoContent();
        }
    }
}
