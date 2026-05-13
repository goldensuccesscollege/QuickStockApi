using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using QuickStock.Applications.Consumables.Command;
using QuickStock.Applications.Consumables.Queries;
using QuickStock.Domain.Consumables;
using System;
using System.Threading.Tasks;

namespace QuickStock.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class ConsumablesController : ControllerBase
    {
        private readonly IMediator _mediator;

        public ConsumablesController(IMediator mediator)
        {
            _mediator = mediator;
        }

        [HttpGet]
        public async Task<IActionResult> GetConsumables(int? campusId = null, string? searchTerm = null, int page = 1, int pageSize = 10)
        {
            var result = await _mediator.Send(new GetConsumablesQuery(campusId, searchTerm, page, pageSize));
            return Ok(result);
        }

        [HttpGet("items/{consumableId}")]
        public async Task<IActionResult> GetConsumableItems(int consumableId)
        {
            var result = await _mediator.Send(new GetConsumableItemsQuery(consumableId));
            return Ok(result);
        }

        [HttpPost]
        [Authorize(Roles = "Admin,Library Admin,Home Economics Admin,Manager,User")]
        public async Task<IActionResult> Create(ConsumableData consumable)
        {
            try
            {
                var result = await _mediator.Send(new CreateConsumableCommand(consumable, User));
                return Ok(result);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpPost("item/status")]
        [Authorize(Roles = "Admin,Library Admin,Home Economics Admin,Manager,User")]
        public async Task<IActionResult> UpdateItemStatus(int itemId, string status)
        {
            var success = await _mediator.Send(new UpdateConsumableItemStatusCommand(itemId, status, User));
            if (!success) return NotFound();
            return NoContent();
        }
    }
}
