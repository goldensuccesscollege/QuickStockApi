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
        public async Task<IActionResult> GetConsumables(int? campusId = null, string? searchTerm = null, int page = 1, int pageSize = 10, bool showOutOnly = false)
        {
            var result = await _mediator.Send(new GetConsumablesQuery(campusId, searchTerm, page, pageSize, showOutOnly));
            return Ok(result);
        }

        [HttpGet("items/{consumableId}")]
        public async Task<IActionResult> GetConsumableItems(int consumableId, bool showOutOnly = false)
        {
            var result = await _mediator.Send(new GetConsumableItemsQuery(consumableId, showOutOnly));
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

        [HttpPut]
        [Authorize(Roles = "Admin,Library Admin,Home Economics Admin,Manager,User")]
        public async Task<IActionResult> Update(ConsumableData consumable)
        {
            var success = await _mediator.Send(new UpdateConsumableCommand(consumable, User));
            return success ? Ok() : NotFound();
        }

        [HttpPost("{id}/restock")]
        [Authorize(Roles = "Admin,Library Admin,Home Economics Admin,Manager,User")]
        public async Task<IActionResult> Restock(int id, [FromBody] int quantity)
        {
            var success = await _mediator.Send(new RestockConsumableCommand(id, quantity, User));
            return success ? Ok() : BadRequest();
        }

        // --- Out Request Workflow ---

        [HttpGet("out-requests")]
        [Authorize(Roles = "Admin,Library Admin,Home Economics Admin,Manager")]
        public async Task<IActionResult> GetPendingOutRequests(int campusId)
        {
            var result = await _mediator.Send(new GetPendingOutRequestsQuery(campusId));
            return Ok(result);
        }

        [HttpGet("out-requests/history")]
        [Authorize(Roles = "Admin,Library Admin,Home Economics Admin,Manager,User")]
        public async Task<IActionResult> GetOutRequestHistory(int campusId, string? status = null, string? userId = null)
        {
            var result = await _mediator.Send(new GetOutRequestHistoryQuery(campusId, status, userId));
            return Ok(result);
        }

        [HttpPost("item/request-out")]
        [Authorize(Roles = "Admin,Library Admin,Home Economics Admin,Manager,User")]
        public async Task<IActionResult> RequestItemOut(int itemId)
        {
            var (success, message) = await _mediator.Send(new RequestItemOutCommand(itemId, User));
            return success ? Ok(new { message }) : BadRequest(new { message });
        }

        [HttpPost("out-requests/{requestId}/approve")]
        [Authorize(Roles = "Admin,Library Admin,Home Economics Admin,Manager")]
        public async Task<IActionResult> ApproveOutRequest(int requestId)
        {
            var (success, message) = await _mediator.Send(new ApproveOutRequestCommand(requestId, User));
            return success ? Ok(new { message }) : BadRequest(new { message });
        }

        [HttpPost("out-requests/{requestId}/reject")]
        [Authorize(Roles = "Admin,Library Admin,Home Economics Admin,Manager")]
        public async Task<IActionResult> RejectOutRequest(int requestId, [FromBody] string reason)
        {
            var (success, message) = await _mediator.Send(new RejectOutRequestCommand(requestId, reason ?? "", User));
            return success ? Ok(new { message }) : BadRequest(new { message });
        }
        
        [HttpPost("out-requests/{requestId}/cancel")]
        [Authorize(Roles = "Admin,Library Admin,Home Economics Admin,Manager,User")]
        public async Task<IActionResult> CancelOutRequest(int requestId)
        {
            var (success, message) = await _mediator.Send(new CancelOutRequestCommand(requestId, User));
            return success ? Ok(new { message }) : BadRequest(new { message });
        }
    }
}
