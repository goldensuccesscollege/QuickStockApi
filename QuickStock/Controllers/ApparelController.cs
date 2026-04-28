using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using QuickStock.Infrastructure.Data;
using Microsoft.AspNetCore.Authorization;
using QuickStock.Domain.Apparel;
using QuickStock.Domain.ITassets;

namespace QuickStock.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class ApparelController : ControllerBase
    {
        private readonly AppDbContext _context;

        public ApparelController(AppDbContext context)
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
                EntityType = "Apparel",
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
        public async Task<ActionResult<IEnumerable<Appareldata>>> GetApparel(int? campusId = null, string? searchTerm = null)
        {
            IQueryable<Appareldata> query = _context.ApparelList.Include(a => a.Campus);

            if (campusId.HasValue && campusId.Value > 0)
            {
                query = query.Where(a => a.CampusId == campusId.Value);
            }

            if (!string.IsNullOrWhiteSpace(searchTerm))
            {
                searchTerm = searchTerm.Trim().ToLower();
                query = query.Where(a => 
                    a.Apparel_Name.ToLower().Contains(searchTerm) || 
                    a.Category.ToLower().Contains(searchTerm) ||
                    a.Supplier_Name.ToLower().Contains(searchTerm));
            }

            var apparel = await query.ToListAsync();
            return apparel;
        }

        [HttpGet("{id}/items")]
        public async Task<ActionResult<IEnumerable<ApparelItem>>> GetApparelItems(int id)
        {
            var items = await _context.ApparelItems
                .Where(i => i.AppareldataId == id)
                .ToListAsync();
            return items;
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<Appareldata>> GetApparel(int id)
        {
            var apparel = await _context.ApparelList.Include(a => a.Campus).FirstOrDefaultAsync(a => a.Apparel_ID == id);
            if (apparel == null) return NotFound();
            return apparel;
        }

        [HttpPost]
        public async Task<IActionResult> Create(Appareldata apparel)
        {
            using var transaction = await _context.Database.BeginTransactionAsync();
            try
            {
                _context.ApparelList.Add(apparel);
                await _context.SaveChangesAsync();

                // Automatically generate individual items based on quantity
                var items = new List<ApparelItem>();
                for (int i = 1; i <= apparel.Quality_In_Stock; i++)
                {
                    items.Add(new ApparelItem
                    {
                        AppareldataId = apparel.Apparel_ID,
                        Apparel_Number = $"AP-{apparel.Apparel_ID}-{DateTime.Now.Year}-{i:D4}",
                        Status = "In Stock",
                        CampusId = apparel.CampusId
                    });
                }
                _context.ApparelItems.AddRange(items);
                await _context.SaveChangesAsync();

                await transaction.CommitAsync();

                await LogAction("Add", apparel.Apparel_ID, apparel.Apparel_Name, $"Added new apparel type: {apparel.Apparel_Name} with {apparel.Quality_In_Stock} items.", apparel.CampusId);

                return CreatedAtAction(nameof(GetApparel), new { id = apparel.Apparel_ID }, apparel);
            }
            catch (Exception)
            {
                await transaction.RollbackAsync();
                throw;
            }
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> Update(int id, Appareldata apparel)
        {
            if (id != apparel.Apparel_ID) return BadRequest();

            var existingApparel = await _context.ApparelList.FindAsync(id);
            if (existingApparel == null) return NotFound();

            existingApparel.Apparel_Name = apparel.Apparel_Name;
            existingApparel.Category = apparel.Category;
            existingApparel.Gender = apparel.Gender;
            existingApparel.Size = apparel.Size;
            existingApparel.Grade_Level = apparel.Grade_Level;
            existingApparel.Quality_In_Stock = apparel.Quality_In_Stock;
            existingApparel.Reorder_level = apparel.Reorder_level;
            existingApparel.Date_Purchased = apparel.Date_Purchased;
            existingApparel.Unit_Price = apparel.Unit_Price;
            existingApparel.Supplier_Name = apparel.Supplier_Name;
            existingApparel.Remarks = apparel.Remarks;

            try
            {
                await _context.SaveChangesAsync();
                await LogAction("Update", apparel.Apparel_ID, apparel.Apparel_Name, $"Updated apparel details", apparel.CampusId);
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!ApparelExists(id)) return NotFound();
                else throw;
            }

            return NoContent();
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            var apparel = await _context.ApparelList.FindAsync(id);
            if (apparel == null) return NotFound();

            var apparelName = apparel.Apparel_Name;
            var campusId = apparel.CampusId;

            _context.ApparelList.Remove(apparel);
            await _context.SaveChangesAsync();

            await LogAction("Delete", id, apparelName, $"Deleted apparel: {apparelName}", campusId);

            return NoContent();
        }

        private bool ApparelExists(int id)
        {
            return _context.ApparelList.Any(e => e.Apparel_ID == id);
        }
    }
}
