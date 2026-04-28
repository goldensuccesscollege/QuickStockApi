using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using QuickStock.Infrastructure.Data;
using Microsoft.AspNetCore.Authorization;
using QuickStock.Domain.ITassets;

namespace QuickStock.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class ItassetsController : ControllerBase
    {
        private readonly AppDbContext _context;

        public ItassetsController(AppDbContext context)
        {
            _context = context;
        }

        private async Task LogAction(string action, int entityId, string entityName, string details, int campusId)
        {
            var userId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
            var username = User.Identity?.Name;
            
            var log = new AuditLog
            {
                Action = action,
                EntityType = "ItAsset",
                EntityId = entityId,
                EntityName = entityName,
                Details = details,
                UserId = userId,
                Username = username,
                CampusId = campusId
            };
            _context.AuditLogs.Add(log);
            await _context.SaveChangesAsync();
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<ItAsset>>> GetItassets(int? roomId = null, int? campusId = null, string? searchTerm = null)
        {
            IQueryable<ItAsset> query = _context.Itassets.Include(a => a.Room);

            // If user is not Admin, hide assets in disabled rooms
            if (!User.IsInRole("Admin"))
            {
                query = query.Where(a => a.RoomId == null || !a.Room!.IsDisabled);
            }

            if (campusId.HasValue && campusId.Value > 0)
            {
                query = query.Where(a => a.CampusId == campusId.Value);
            }

            if (roomId.HasValue && roomId.Value > 0)
            {
                query = query.Where(a => a.RoomId == roomId.Value);
            }

            if (!string.IsNullOrWhiteSpace(searchTerm))
            {
                searchTerm = searchTerm.Trim().ToLower();
                query = query.Where(a => 
                    a.Name.ToLower().Contains(searchTerm) || 
                    (a.Brand != null && a.Brand.ToLower().Contains(searchTerm)) || 
                    (a.SerialNumber != null && a.SerialNumber.ToLower().Contains(searchTerm)));
            }

            var assets = await query.ToListAsync();
            return assets;
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<ItAsset>> GetItAsset(int id)
        {
            var itAsset = await _context.Itassets.Include(a => a.Room).FirstOrDefaultAsync(a => a.Id == id);
            if (itAsset == null) return NotFound();

            if (itAsset.Room != null && itAsset.Room.IsDisabled && !User.IsInRole("Admin"))
            {
                return Forbid();
            }

            return itAsset;
        }

        [HttpPost]
        public async Task<IActionResult> Create(ItAsset asset)
        {
            if (!string.IsNullOrWhiteSpace(asset.SerialNumber))
            {
                var duplicate = await _context.Itassets.AnyAsync(a => a.SerialNumber == asset.SerialNumber && a.CampusId == asset.CampusId);
                if (duplicate) return BadRequest("An item with this serial number already exists in this campus.");
            }

            if (string.IsNullOrWhiteSpace(asset.Qrcode))
            {
                asset.Qrcode = Guid.NewGuid().ToString("N").Substring(0, 12).ToUpper();
            }

            asset.DateAdded = DateTime.UtcNow;
            _context.Itassets.Add(asset);
            await _context.SaveChangesAsync();

            await LogAction("Add", asset.Id, asset.Name, $"Added new {asset.Type}: {asset.Name} (Auto-QR: {asset.Qrcode})", asset.CampusId);

            return CreatedAtAction(nameof(GetItAsset), new { id = asset.Id }, asset);
        }

        [HttpGet("qr/{qrCode}")]
        [AllowAnonymous]
        public async Task<ActionResult<ItAsset>> GetByQrCode(string qrCode)
        {
            var asset = await _context.Itassets
                .Include(a => a.Room)
                .Include(a => a.Campus)
                .FirstOrDefaultAsync(a => a.Qrcode == qrCode);

            if (asset == null) return NotFound();

            return asset;
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> Update(int id, ItAsset asset)
        {
            if (id != asset.Id) return BadRequest();

            if (!string.IsNullOrWhiteSpace(asset.SerialNumber))
            {
                var duplicate = await _context.Itassets.AnyAsync(a => a.SerialNumber == asset.SerialNumber && a.CampusId == asset.CampusId && a.Id != id);
                if (duplicate) return BadRequest("An item with this serial number already exists in this campus.");
            }


            var existingAsset = await _context.Itassets.FindAsync(id);
            if (existingAsset == null) return NotFound();

            existingAsset.Name = asset.Name;
            existingAsset.Type = asset.Type;
            existingAsset.Brand = asset.Brand;
            existingAsset.Model = asset.Model;
            existingAsset.Location = asset.Location;
            existingAsset.Status = asset.Status;
            existingAsset.Qrcode = asset.Qrcode;
            existingAsset.RoomId = asset.RoomId;

            try
            {
                await _context.SaveChangesAsync();
                await LogAction("Update", asset.Id, asset.Name, $"Updated asset details", asset.CampusId);
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!AssetExists(id)) return NotFound();
                else throw;
            }

            return NoContent();
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            var asset = await _context.Itassets.FindAsync(id);
            if (asset == null) return NotFound();

            var assetName = asset.Name;
            var campusId = asset.CampusId;

            _context.Itassets.Remove(asset);
            await _context.SaveChangesAsync();

            await LogAction("Delete", id, assetName, $"Deleted asset: {assetName}", campusId);

            return NoContent();
        }

        [HttpPost("{id}/transfer")]
        public async Task<IActionResult> Transfer(int id, [FromBody] int targetRoomId)
        {
            var asset = await _context.Itassets.Include(a => a.Room).FirstOrDefaultAsync(a => a.Id == id);
            if (asset == null) return NotFound();

            var oldRoomName = asset.Room?.RoomName ?? "Unknown";
            var targetRoom = await _context.Rooms.FindAsync(targetRoomId);
            if (targetRoom == null) return BadRequest("Target room not found.");

            asset.RoomId = targetRoomId;
            asset.Location = targetRoom.RoomName;
            await _context.SaveChangesAsync();

            await LogAction("Transfer", asset.Id, asset.Name, $"Transferred from {oldRoomName} to {targetRoom.RoomName}", asset.CampusId);

            return Ok(new { success = true, roomName = targetRoom.RoomName });
        }

        private bool AssetExists(int id)
        {
            return _context.Itassets.Any(e => e.Id == id);
        }
    }
}
