using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using QuickStock.Domain.ITassets;
using QuickStock.Infrastructure.Data;

namespace QuickStock.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Microsoft.AspNetCore.Authorization.Authorize]
    public class CampusesController : ControllerBase
    {
        private readonly AppDbContext _context;

        public CampusesController(AppDbContext context)
        {
            _context = context;
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<Campus>>> GetCampuses()
        {
            return await _context.Campuses.ToListAsync();
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<Campus>> GetCampus(int id)
        {
            var campus = await _context.Campuses.FindAsync(id);
            if (campus == null) return NotFound();
            return campus;
        }

        [HttpPost]
        [Microsoft.AspNetCore.Authorization.Authorize(Roles = "Admin")]
        public async Task<ActionResult<Campus>> CreateCampus(Campus campus)
        {
            _context.Campuses.Add(campus);
            await _context.SaveChangesAsync();
            return CreatedAtAction(nameof(GetCampus), new { id = campus.CampusId }, campus);
        }

        [HttpPut("{id}")]
        [Microsoft.AspNetCore.Authorization.Authorize(Roles = "Admin")]
        public async Task<IActionResult> UpdateCampus(int id, Campus campus)
        {
            if (id != campus.CampusId) return BadRequest();
            _context.Entry(campus).State = EntityState.Modified;
            try { await _context.SaveChangesAsync(); }
            catch (DbUpdateConcurrencyException) { if (!CampusExists(id)) return NotFound(); else throw; }
            return NoContent();
        }

        [HttpDelete("{id}")]
        [Microsoft.AspNetCore.Authorization.Authorize(Roles = "Admin")]
        public async Task<IActionResult> DeleteCampus(int id)
        {
            var campus = await _context.Campuses.FindAsync(id);
            if (campus == null) return NotFound();
            _context.Campuses.Remove(campus);
            await _context.SaveChangesAsync();
            return NoContent();
        }

        private bool CampusExists(int id) => _context.Campuses.Any(e => e.CampusId == id);
    }
}
