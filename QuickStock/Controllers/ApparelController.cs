using QuickStock.CQRS;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using QuickStock.Applications.Apparel.Command;
using QuickStock.Applications.Apparel.Queries;
using QuickStock.Domain.Apparel;

namespace QuickStock.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class ApparelController : ControllerBase
    {
        private readonly IMediator _mediator;

        public ApparelController(IMediator mediator)
        {
            _mediator = mediator;
        }

        [HttpGet]
        public async Task<IActionResult> GetApparel(int? campusId = null, string? searchTerm = null, int page = 1, int pageSize = 5)
        {
            var result = await _mediator.Send(new GetApparelQuery(campusId, searchTerm, page, pageSize));
            return Ok(result);
        }

        [HttpGet("sold")]
        public async Task<IActionResult> GetSoldItems(int? campusId = null, int page = 1, int pageSize = 10)
        {
            var result = await _mediator.Send(new GetSoldItemsQuery(campusId, page, pageSize));
            return Ok(result);
        }

        [HttpGet("items/{apparelId}")]
        public async Task<IActionResult> GetApparelItems(int apparelId)
        {
            var result = await _mediator.Send(new GetApparelItemsQuery(apparelId));
            return Ok(result);
        }

        [HttpGet("items/query")]
        public async Task<IActionResult> QueryItems(string? status = null, DateTime? startDate = null, DateTime? endDate = null, int? campusId = null)
        {
            var result = await _mediator.Send(new QueryItemsQuery(status, startDate, endDate, campusId));
            return Ok(result);
        }

        [HttpGet("item/qr/{qr}")]
        public async Task<ActionResult<ApparelItem>> GetApparelItemByQr(string qr)
        {
            var result = await _mediator.Send(new GetApparelItemByQrQuery(qr));
            if (result == null) return NotFound();
            return Ok(result);
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<Appareldata>> GetApparel(int id)
        {
            var result = await _mediator.Send(new GetApparelByIdQuery(id));
            if (result == null) return NotFound();
            return Ok(result);
        }

        [HttpPost]
        [Authorize(Roles = "Admin,Manager,Employee")]
        public async Task<IActionResult> Create(Appareldata apparel)
        {
            try
            {
                var result = await _mediator.Send(new CreateApparelCommand(apparel, User));
                return CreatedAtAction(nameof(GetApparel), new { id = result.Apparel_ID }, result);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpPost("{id}/add-stock")]
        [Authorize(Roles = "Admin,Manager,Employee")]
        public async Task<IActionResult> AddStock(int id, [FromBody] int additionalQuantity)
        {
            if (additionalQuantity <= 0) return BadRequest("Quantity must be greater than zero.");
            try
            {
                var result = await _mediator.Send(new AddStockCommand(id, additionalQuantity, User));
                return Ok(result);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpPut("{id}")]
        [Authorize(Roles = "Admin,Manager")]
        public async Task<IActionResult> Update(int id, Appareldata apparel)
        {
            if (id != apparel.Apparel_ID) return BadRequest();
            var success = await _mediator.Send(new UpdateApparelCommand(id, apparel, User));
            if (!success) return NotFound();
            return NoContent();
        }

        [HttpDelete("{id}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Delete(int id)
        {
            var success = await _mediator.Send(new DeleteApparelCommand(id, User));
            if (!success) return NotFound();
            return NoContent();
        }

        [HttpPost("item/status")]
        public async Task<IActionResult> UpdateItemStatus(int itemId, string status)
        {
            var success = await _mediator.Send(new UpdateItemStatusCommand(itemId, status, User));
            if (!success) return NotFound();
            return NoContent();
        }
    }
}
