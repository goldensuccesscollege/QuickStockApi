using QuickStock.CQRS;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using QuickStock.Applications.Itassets.Command;
using QuickStock.Applications.Itassets.Queries;
using QuickStock.Domain.ITassets;
using QuickStock.Domain.Locations;
using QuickStock.Domain.Shared;
using QuickStock.Domain.Accounts;

namespace QuickStock.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class ItassetsController : ControllerBase
    {
        private readonly IMediator _mediator;

        public ItassetsController(IMediator mediator)
        {
            _mediator = mediator;
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<ItAsset>>> GetItassets(int? roomId = null, int? campusId = null, string? searchTerm = null)
        {
            var result = await _mediator.Send(new GetItAssetsQuery(roomId, campusId, searchTerm, User));
            return Ok(result);
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<ItAsset>> GetItAsset(int id)
        {
            var asset = await _mediator.Send(new GetItAssetByIdQuery(id, User));
            if (asset == null) return NotFound();
            return Ok(asset);
        }

        [HttpGet("qr/{qrCode}")]
        [AllowAnonymous]
        public async Task<ActionResult<ItAsset>> GetByQrCode(string qrCode)
        {
            var asset = await _mediator.Send(new GetItAssetByQrQuery(qrCode));
            if (asset == null) return NotFound();
            return Ok(asset);
        }

        [HttpPost]
        [Authorize(Roles = "Admin,Manager,Staff")]
        public async Task<IActionResult> Create(ItAsset asset)
        {
            try
            {
                var result = await _mediator.Send(new CreateItAssetCommand(asset, User));
                return CreatedAtAction(nameof(GetItAsset), new { id = result.Id }, result);
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpPut("{id}")]
        [Authorize(Roles = "Admin,Manager")]
        public async Task<IActionResult> Update(int id, ItAsset asset)
        {
            if (id != asset.Id) return BadRequest();
            try
            {
                var success = await _mediator.Send(new UpdateItAssetCommand(id, asset, User));
                if (!success) return NotFound();
                return NoContent();
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpDelete("{id}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Delete(int id)
        {
            var success = await _mediator.Send(new DeleteItAssetCommand(id, User));
            if (!success) return NotFound();
            return NoContent();
        }

        [HttpPost("{id}/transfer")]
        public async Task<IActionResult> Transfer(int id, [FromBody] int targetRoomId)
        {
            try
            {
                var result = await _mediator.Send(new TransferItAssetCommand(id, targetRoomId, User));
                return Ok(result);
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(ex.Message);
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(ex.Message);
            }
        }
    }
}
