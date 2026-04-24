using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using QuickStock.Domain;
using QuickStock.Infrastructure.Data;
using Microsoft.AspNetCore.Authorization;

namespace QuickStock.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class RoomsController : ControllerBase
    {
        private readonly AppDbContext _context;

        public RoomsController(AppDbContext context)
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
                EntityType = "Room",
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
        public async Task<ActionResult<IEnumerable<Room>>> GetRooms(int? campusId = null)
        {
            IQueryable<Room> query = _context.Rooms;
            
            // If user is not Admin, hide disabled rooms
            if (!User.IsInRole("Admin"))
            {
                query = query.Where(r => !r.IsDisabled);
            }

            if (campusId.HasValue && campusId.Value > 0)
            {
                query = query.Where(r => r.CampusId == campusId.Value);
            }
            return await query.ToListAsync();
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<Room>> GetRoom(int id)
        {
            var room = await _context.Rooms.FindAsync(id);
            if (room == null) return NotFound();
            
            if (room.IsDisabled && !User.IsInRole("Admin"))
            {
                return Forbid();
            }

            return room;
        }

        [HttpPost]
        public async Task<ActionResult<Room>> CreateRoom(Room room)
        {
            _context.Rooms.Add(room);
            await _context.SaveChangesAsync();

            await LogAction("Add", room.RoomId, room.RoomName, $"Created room: {room.RoomName} on {room.RoomFloor}", room.CampusId);

            return CreatedAtAction(nameof(GetRoom), new { id = room.RoomId }, room);
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateRoom(int id, Room room)
        {
            if (id != room.RoomId) return BadRequest();
            _context.Entry(room).State = EntityState.Modified;
            try { 
                await _context.SaveChangesAsync(); 
                await LogAction("Update", room.RoomId, room.RoomName, $"Updated room details", room.CampusId);
            }
            catch (DbUpdateConcurrencyException) { if (!RoomExists(id)) return NotFound(); else throw; }
            return NoContent();
        }

        [HttpPut("{id}/toggle-status")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> ToggleStatus(int id)
        {
            var room = await _context.Rooms.FindAsync(id);
            if (room == null) return NotFound();

            room.IsDisabled = !room.IsDisabled;
            await _context.SaveChangesAsync();

            await LogAction("ToggleStatus", room.RoomId, room.RoomName, $"Room status changed to {(room.IsDisabled ? "Disabled" : "Enabled")}", room.CampusId);

            return Ok(new { isDisabled = room.IsDisabled });
        }

        [HttpDelete("{id}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> DeleteRoom(int id)
        {
            var room = await _context.Rooms.FindAsync(id);
            if (room == null) return NotFound();
            
            var roomName = room.RoomName;
            var campusId = room.CampusId;

            _context.Rooms.Remove(room);
            await _context.SaveChangesAsync();

            await LogAction("Delete", id, roomName, $"Deleted room: {roomName}", campusId);

            return NoContent();
        }

        private bool RoomExists(int id) => _context.Rooms.Any(e => e.RoomId == id);
    }
}
