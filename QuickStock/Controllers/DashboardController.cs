using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using QuickStock.Infrastructure.Data;
using Microsoft.AspNetCore.Authorization;

namespace QuickStock.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class DashboardController : ControllerBase
    {
        private readonly AppDbContext _context;

        public DashboardController(AppDbContext context)
        {
            _context = context;
        }

        [HttpGet("{campusId}/stats")]
        public async Task<IActionResult> GetStats(int campusId)
        {
            var totalAssets = await _context.Itassets.CountAsync(a => a.CampusId == campusId);
            var roomCount = await _context.Rooms.CountAsync(r => r.CampusId == campusId);
            var disabledRooms = await _context.Rooms.CountAsync(r => r.CampusId == campusId && r.IsDisabled);
            
            var recentLogs = await _context.AuditLogs
                .Where(l => l.CampusId == campusId)
                .OrderByDescending(l => l.Timestamp)
                .Take(5)
                .Select(l => new {
                    l.Action,
                    l.EntityName,
                    Timestamp = l.Timestamp.ToString("yyyy-MM-dd HH:mm"),
                    l.Username,
                    l.Details
                })
                .ToListAsync();

            return Ok(new
            {
                TotalAssets = totalAssets,
                RoomCount = roomCount,
                DisabledRooms = disabledRooms,
                RecentActivities = recentLogs
            });
        }
    }
}
