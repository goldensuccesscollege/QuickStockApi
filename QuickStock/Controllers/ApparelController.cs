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
        public async Task<IActionResult> GetApparel(int? campusId = null, string? searchTerm = null, int page = 1, int pageSize = 5)
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

            var totalItems = await query.CountAsync();
            var apparel = await query
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            return Ok(new 
            { 
                TotalItems = totalItems, 
                Apparel = apparel,
                Page = page,
                PageSize = pageSize,
                TotalPages = (int)Math.Ceiling((double)totalItems / pageSize)
            });
        }

        [HttpGet("sold")]
        public async Task<IActionResult> GetSoldItems(int? campusId = null, int page = 1, int pageSize = 10)
        {
            IQueryable<ApparelItem> query = _context.ApparelItems
                .Include(i => i.ApparelType)
                .Where(i => i.Status == "Sold");

            if (campusId.HasValue && campusId.Value > 0)
            {
                query = query.Where(i => i.CampusId == campusId.Value);
            }

            var totalItems = await query.CountAsync();
            var items = await query
                .OrderByDescending(i => i.Id)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            return Ok(new 
            { 
                TotalItems = totalItems, 
                Items = items,
                Page = page,
                PageSize = pageSize,
                TotalPages = (int)Math.Ceiling((double)totalItems / pageSize)
            });
        }

        [HttpGet("items/{apparelId}")]
        public async Task<IActionResult> GetApparelItems(int apparelId)
        {
            var items = await _context.ApparelItems
                .Where(i => i.AppareldataId == apparelId)
                .ToListAsync();
            return Ok(items);
        }

        [HttpGet("items/query")]
        public async Task<IActionResult> QueryItems(string? status = null, DateTime? startDate = null, DateTime? endDate = null, int? campusId = null)
        {
            IQueryable<ApparelItem> query = _context.ApparelItems.Include(i => i.ApparelType);

            if (!string.IsNullOrEmpty(status))
            {
                query = query.Where(i => i.Status == status);
            }

            if (campusId.HasValue && campusId.Value > 0)
            {
                query = query.Where(i => i.CampusId == campusId.Value);
            }

            if (startDate.HasValue)
            {
                query = query.Where(i => i.LastModified >= startDate.Value);
            }

            if (endDate.HasValue)
            {
                var endOfDate = endDate.Value.Date.AddDays(1).AddTicks(-1);
                query = query.Where(i => i.LastModified <= endOfDate);
            }

            var items = await query.OrderByDescending(i => i.LastModified).ToListAsync();
            return Ok(items);
        }

        [HttpGet("item/qr/{qr}")]
        public async Task<ActionResult<ApparelItem>> GetApparelItemByQr(string qr)
        {
            var item = await _context.ApparelItems
                .Include(i => i.ApparelType)
                .Include(i => i.Campus)
                .FirstOrDefaultAsync(i => i.Apparel_Number == qr);

            if (item == null) return NotFound();
            return item;
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
                var nextNum = 1;
                var items = new List<ApparelItem>();
                for (int i = 1; i <= apparel.Quality_In_Stock; i++)
                {
                    items.Add(new ApparelItem
                    {
                        ApparelType = apparel,
                        Apparel_Number = $"{apparel.Apparel_Name.Substring(0, 3).ToUpper()}-{nextNum++:D4}",
                        Status = "In Stock",
                        CampusId = apparel.CampusId,
                        DateCreated = DateTime.UtcNow,
                        LastModified = DateTime.UtcNow
                    });
                }
                _context.ApparelItems.AddRange(items);
                await _context.SaveChangesAsync();

                await transaction.CommitAsync();

                await LogAction("Register", apparel.Apparel_ID, apparel.Apparel_Name, $"Registered {apparel.Quality_In_Stock} items", apparel.CampusId);

                return CreatedAtAction(nameof(GetApparel), new { id = apparel.Apparel_ID }, apparel);
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                return BadRequest(ex.Message);
            }
        }

        [HttpPost("{id}/add-stock")]
        public async Task<IActionResult> AddStock(int id, [FromBody] int additionalQuantity)
        {
            if (additionalQuantity <= 0) return BadRequest("Quantity must be greater than zero.");

            using var transaction = await _context.Database.BeginTransactionAsync();
            try
            {
                var apparel = await _context.ApparelList.FindAsync(id);
                if (apparel == null) return NotFound();

                // Get current item count to determine next sequence number
                var currentItemsCount = await _context.ApparelItems
                    .Where(i => i.AppareldataId == id)
                    .CountAsync();

                var newItems = new List<ApparelItem>();
                for (int i = 1; i <= additionalQuantity; i++)
                {
                    newItems.Add(new ApparelItem
                    {
                        ApparelType = apparel,
                        Apparel_Number = $"AP-{apparel.Apparel_ID}-{DateTime.Now.Year}-{(currentItemsCount + i):D4}",
                        Status = "In Stock",
                        CampusId = apparel.CampusId,
                        DateCreated = DateTime.UtcNow,
                        LastModified = DateTime.UtcNow
                    });
                }

                _context.ApparelItems.AddRange(newItems);
                apparel.Quality_In_Stock += additionalQuantity;
                
                await _context.SaveChangesAsync();
                await transaction.CommitAsync();

                await LogAction("Add Stock", apparel.Apparel_ID, apparel.Apparel_Name, $"Added {additionalQuantity} more items", apparel.CampusId);

                return Ok(new { success = true, newQuantity = apparel.Quality_In_Stock });
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                return BadRequest(ex.Message);
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
            existingApparel.Sex = apparel.Sex;
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

        [HttpPost("item/status")]
        public async Task<IActionResult> UpdateItemStatus(int itemId, string status)
        {
            var newStatus = status;
            var item = await _context.ApparelItems.Include(i => i.ApparelType).FirstOrDefaultAsync(i => i.Id == itemId);
            if (item == null) return NotFound();

            var oldStatus = item.Status;
            item.Status = newStatus;
            item.LastModified = DateTime.UtcNow;

            try
            {
                // Update availability count
                if (item.ApparelType != null)
                {
                    if (newStatus.Equals("Sold", StringComparison.OrdinalIgnoreCase) && !oldStatus.Equals("Sold", StringComparison.OrdinalIgnoreCase))
                    {
                        item.ApparelType.Quality_In_Stock--;
                    }
                    else if (!newStatus.Equals("Sold", StringComparison.OrdinalIgnoreCase) && oldStatus.Equals("Sold", StringComparison.OrdinalIgnoreCase))
                    {
                        item.ApparelType.Quality_In_Stock++;
                    }
                }

                await _context.SaveChangesAsync();

                if (newStatus.Equals("Sold", StringComparison.OrdinalIgnoreCase))
                {
                    var apparel = item.ApparelType;
                    await LogAction("sold", item.Id, item.Apparel_Number, 
                        $"Apparel Name: {apparel.Apparel_Name}, Apparel Number: {item.Apparel_Number}, Category: {apparel.Category}, Sex: {apparel.Sex}, Size: {apparel.Size}, Grade Level: {apparel.Grade_Level}, Unit Price: {apparel.Unit_Price:C}", 
                        item.CampusId);
                }
                else
                {
                    await LogAction("UpdateStatus", item.Id, item.Apparel_Number, $"Changed status from {oldStatus} to {newStatus}", item.CampusId);
                }
            }
            catch (DbUpdateException)
            {
                throw;
            }

            return NoContent();
        }
    }
}
