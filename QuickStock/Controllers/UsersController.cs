using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using QuickStock.Applications.Users.Command;
using QuickStock.Applications.Users.Dtos;
using QuickStock.Applications.Users.Queries;

namespace QuickStock.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize(Roles = "Admin,Library Admin,Home Economics Admin")]
    public class UsersController : ControllerBase
    {
        private readonly IMediator _mediator;

        public UsersController(IMediator mediator)
        {
            _mediator = mediator;
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<object>>> GetUsers()
        {
            var result = await _mediator.Send(new GetUsersQuery());
            return Ok(result);
        }

        [HttpPost]
        public async Task<IActionResult> CreateUser([FromBody] CreateUserDto dto)
        {
            try
            {
                var result = await _mediator.Send(new CreateUserCommand(dto));
                return Ok(result);
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateUser(int id, [FromBody] UpdateUserDto dto)
        {
            var success = await _mediator.Send(new UpdateUserCommand(id, dto));
            if (!success) return NotFound();
            return Ok(new { message = "User updated successfully" });
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteUser(int id)
        {
            var success = await _mediator.Send(new DeleteUserCommand(id));
            if (!success) return NotFound();
            return Ok(new { message = "User deleted successfully" });
        }

        [HttpPut("{id}/toggle-status")]
        public async Task<IActionResult> ToggleUserStatus(int id)
        {
            try
            {
                var result = await _mediator.Send(new ToggleUserStatusCommand(id));
                return Ok(result);
            }
            catch (KeyNotFoundException) { return NotFound(); }
        }

        [HttpPost("{userId}/campuses")]
        public async Task<IActionResult> AddCampusAccess(int userId, [FromBody] int campusId)
        {
            await _mediator.Send(new AddCampusAccessCommand(userId, campusId));
            return Ok();
        }

        [HttpDelete("{userId}/campuses/{campusId}")]
        public async Task<IActionResult> RemoveCampusAccess(int userId, int campusId)
        {
            var success = await _mediator.Send(new RemoveCampusAccessCommand(userId, campusId));
            if (!success) return NotFound();
            return Ok();
        }

        [HttpPut("{userId}/campuses/{campusId}/toggle-block")]
        public async Task<IActionResult> ToggleBlock(int userId, int campusId)
        {
            try
            {
                var result = await _mediator.Send(new ToggleCampusBlockCommand(userId, campusId));
                return Ok(result);
            }
            catch (KeyNotFoundException) { return NotFound(); }
        }

        [HttpPut("{id}/toggle-it-access")]
        public async Task<IActionResult> ToggleITAccess(int id)
        {
            try
            {
                var result = await _mediator.Send(new ToggleITAccessCommand(id));
                return Ok(result);
            }
            catch (KeyNotFoundException) { return NotFound(); }
        }

        [HttpPut("{id}/toggle-ap-access")]
        public async Task<IActionResult> ToggleAPAccess(int id)
        {
            try
            {
                var result = await _mediator.Send(new ToggleApparelAccessCommand(id));
                return Ok(result);
            }
            catch (KeyNotFoundException) { return NotFound(); }
        }

        [HttpPut("{id}/toggle-message-access")]
        public async Task<IActionResult> ToggleMessageAccess(int id)
        {
            try
            {
                var result = await _mediator.Send(new ToggleMessageAccessCommand(id));
                return Ok(result);
            }
            catch (KeyNotFoundException) { return NotFound(); }
        }

        [HttpPut("{id}/toggle-library-access")]
        public async Task<IActionResult> ToggleLibraryAccess(int id)
        {
            try
            {
                var result = await _mediator.Send(new ToggleLibraryAccessCommand(id));
                return Ok(result);
            }
            catch (KeyNotFoundException) { return NotFound(); }
        }

        [HttpPut("{id}/toggle-he-access")]
        public async Task<IActionResult> ToggleHomeEconomicsAccess(int id)
        {
            try
            {
                var result = await _mediator.Send(new ToggleHomeEconomicsAccessCommand(id));
                return Ok(result);
            }
            catch (KeyNotFoundException) { return NotFound(); }
        }

        [HttpPut("{id}/toggle-consumables-access")]
        public async Task<IActionResult> ToggleConsumablesAccess(int id)
        {
            try
            {
                var result = await _mediator.Send(new ToggleConsumablesAccessCommand(id));
                return Ok(result);
            }
            catch (KeyNotFoundException) { return NotFound(); }
        }
    }
}
