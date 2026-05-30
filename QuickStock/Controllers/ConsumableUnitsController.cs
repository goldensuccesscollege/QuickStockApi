using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using QuickStock.Applications.Consumables.Commands;
using QuickStock.Applications.Consumables.Queries; // Imported query namespace
using QuickStock.Applications.Consumables.Dto_s; // Imported DTO response namespace
using QuickStock.CQRS;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace QuickStock.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
       public class ConsumableUnitsController : ControllerBase
    {
        private readonly IMediator _mediator;

        public ConsumableUnitsController(IMediator mediator)
        {
            _mediator = mediator;
        }

        /// <summary>
        /// Retrieves all consumables, optionally filtered by Campus ID.
        /// URL: GET /api/ConsumableUnits?campusId=1
        /// </summary>
        [HttpGet]
        [ProducesResponseType(typeof(List<ConsumableResponse>), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> GetAll([FromQuery] int? campusId)
        {
            var query = new GetConsumablesQuery { CampusId = campusId };
            var result = await _mediator.Send(query);
            
            // Returns HTTP 200 OK along with the populated collection list
            return Ok(result);
        }

        [HttpPost]
        [ProducesResponseType(StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> Create([FromBody] CreateConsumableCommand command)
        {
            // Dispatches execution directly over to the handler safely
            var consumableId = await _mediator.Send(command);
            
            // Returns HTTP 201 Created containing the primary database ID location tag
            return CreatedAtAction(nameof(Create), new { id = consumableId }, new { Id = consumableId });
        }

        [HttpPost("add-stock")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> AddStock([FromBody] AddStockCommand command)
        {
            var result = await _mediator.Send(command);
            
            // Returns standard HTTP 200 OK since we modified an existing resource
            return Ok(result);
        }

        [HttpPost("deduct-stock")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> DeductStock([FromBody] DeductStockCommand command)
        {
            var result = await _mediator.Send(command);
            
            // Returns HTTP 200 OK along with the updated inventory item trace details
            return Ok(result);
        }

        [HttpGet("requests")]
        [ProducesResponseType(typeof(List<ConsumableRequestDto>), StatusCodes.Status200OK)]
        public async Task<IActionResult> GetRequests([FromQuery] int? campusId, [FromQuery] string? status)
        {
            var query = new GetConsumableRequestsQuery { CampusId = campusId, Status = status };
            var result = await _mediator.Send(query);
            return Ok(result);
        }

        [HttpPost("requests")]
        [ProducesResponseType(typeof(ConsumableCreateResponse), StatusCodes.Status200OK)]
        public async Task<IActionResult> CreateRequest([FromBody] CreateConsumableRequestCommand command)
        {
            command.User = User;
            var result = await _mediator.Send(command);
            return Ok(result);
        }

        [HttpPost("requests/{id}/approve")]
        [ProducesResponseType(typeof(ConsumableCreateResponse), StatusCodes.Status200OK)]
        public async Task<IActionResult> ApproveRequest(int id)
        {
            var command = new ApproveConsumableRequestCommand(id, User);
            var result = await _mediator.Send(command);
            return Ok(result);
        }

        [HttpPost("requests/{id}/reject")]
        [ProducesResponseType(typeof(ConsumableCreateResponse), StatusCodes.Status200OK)]
        public async Task<IActionResult> RejectRequest(int id, [FromBody] RejectConsumableRequestCommand command)
        {
            command.Id = id;
            command.User = User;
            var result = await _mediator.Send(command);
            return Ok(result);
        }
    }
}