using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using QuickStock.Applications.Campuses.Command;
using QuickStock.Applications.Campuses.Queries;
using QuickStock.Domain.ITassets;
using QuickStock.Domain.Messaging;
using QuickStock.Domain.Social;
using QuickStock.Domain.Locations;
using QuickStock.Domain.Shared;
using QuickStock.Domain.Accounts;
using QuickStock.Domain.Messaging;
using QuickStock.Domain.Social;
using QuickStock.Domain.Locations;
using QuickStock.Domain.Shared;

namespace QuickStock.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class CampusesController : ControllerBase
    {
        private readonly IMediator _mediator;

        public CampusesController(IMediator mediator) => _mediator = mediator;

        [HttpGet]
        public async Task<ActionResult<IEnumerable<Campus>>> GetCampuses()
        {
            var result = await _mediator.Send(new GetCampusesQuery());
            return Ok(result);
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<Campus>> GetCampus(int id)
        {
            var campus = await _mediator.Send(new GetCampusByIdQuery(id));
            if (campus == null) return NotFound();
            return Ok(campus);
        }

        [HttpPost]
        [Authorize(Roles = "Admin")]
        public async Task<ActionResult<Campus>> CreateCampus(Campus campus)
        {
            var result = await _mediator.Send(new CreateCampusCommand(campus));
            return CreatedAtAction(nameof(GetCampus), new { id = result.CampusId }, result);
        }

        [HttpPut("{id}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> UpdateCampus(int id, Campus campus)
        {
            if (id != campus.CampusId) return BadRequest();
            await _mediator.Send(new UpdateCampusCommand(id, campus));
            return NoContent();
        }

        [HttpDelete("{id}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> DeleteCampus(int id)
        {
            var success = await _mediator.Send(new DeleteCampusCommand(id));
            if (!success) return NotFound();
            return NoContent();
        }
    }
}
