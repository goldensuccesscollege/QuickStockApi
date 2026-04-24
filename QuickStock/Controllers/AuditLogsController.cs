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
    public class AuditLogsController : ControllerBase
    {
        private readonly AppDbContext _context;

        public AuditLogsController(AppDbContext context)
        {
            _context = context;
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<AuditLog>>> GetAuditLogs(int? campusId = null)
        {
            IQueryable<AuditLog> query = _context.AuditLogs.OrderByDescending(l => l.Timestamp);

            if (campusId.HasValue && campusId.Value > 0)
            {
                query = query.Where(l => l.CampusId == campusId.Value);
            }

            return await query.Take(50).ToListAsync();
        }
    }
}
