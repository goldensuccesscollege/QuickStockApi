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
    public class AuditLogsController : ControllerBase
    {
        private readonly AppDbContext _context;

        public AuditLogsController(AppDbContext context)
        {
            _context = context;
        }

        [HttpGet]
        public async Task<IActionResult> GetAuditLogs(int? campusId = null, int page = 1, int pageSize = 10)
        {
            IQueryable<AuditLog> query = _context.AuditLogs.OrderByDescending(l => l.Timestamp);

            if (campusId.HasValue && campusId.Value > 0)
            {
                query = query.Where(l => l.CampusId == campusId.Value);
            }

            var totalItems = await query.CountAsync();
            var logs = await query.Skip((page - 1) * pageSize).Take(pageSize).ToListAsync();

            return Ok(new 
            { 
                totalItems, 
                logs,
                page,
                pageSize,
                totalPages = (int)Math.Ceiling((double)totalItems / pageSize)
            });
        }
    }
}
