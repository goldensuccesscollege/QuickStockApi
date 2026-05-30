using QuickStock.CQRS;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using QuickStock.Applications.Furniture.Command;
using QuickStock.Applications.Furniture.Queries;
using QuickStock.Domain.Furniture;

namespace QuickStock.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class FurnitureController : ControllerBase
    {
        private readonly IMediator _mediator;

        public FurnitureController(IMediator mediator)
        {
            _mediator = mediator;
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<Domain.Furniture.Furniture>>> GetFurnitures(int? roomId = null, int? campusId = null, string? searchTerm = null)
        {
            var result = await _mediator.Send(new GetFurnituresQuery(roomId, campusId, searchTerm, User));
            return Ok(result);
        }

        [HttpPost]
        [Authorize(Roles = "Admin,Manager,Staff")]
        public async Task<IActionResult> Create(Domain.Furniture.Furniture furniture)
        {
            try
            {
                var result = await _mediator.Send(new CreateFurnitureCommand(furniture, User));
                return Ok(result);
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(ex.Message);
            }
        }
        [HttpGet("qr/{qrCode}")]
        [AllowAnonymous]
        public async Task<ActionResult<Domain.Furniture.Furniture>> GetByQrCode(string qrCode)
        {
            var furniture = await _mediator.Send(new GetFurnitureByQrQuery(qrCode));
            if (furniture == null) return NotFound();
            return Ok(furniture);
        }
        [HttpPut]
        [Authorize(Roles = "Admin,Manager")]
        public async Task<IActionResult> Update(Domain.Furniture.Furniture furniture)
        {
            try
            {
                var result = await _mediator.Send(new UpdateFurnitureCommand(furniture, User));
                return Ok(result);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpPost("transfer")]
        public async Task<IActionResult> Transfer([FromBody] TransferRequest request)
        {
            try
            {
                var result = await _mediator.Send(new TransferFurnitureCommand(request.Id, request.TargetRoomId, User));
                return Ok(result);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }
    }

    public record TransferRequest(int Id, int TargetRoomId);
}


