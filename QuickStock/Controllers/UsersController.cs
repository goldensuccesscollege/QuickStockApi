using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using QuickStock.Domain.ITassets;
using QuickStock.Infrastructure.Data;
using Microsoft.AspNetCore.Authorization;

namespace QuickStock.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize(Roles = "Admin")]
    public class UsersController : ControllerBase
    {
        private readonly AppDbContext _context;

        public UsersController(AppDbContext context)
        {
            _context = context;
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<object>>> GetUsers()
        {
            var users = await _context.Accounts
                .Select(a => new
                {
                    a.Id,
                    a.Username,
                    a.Email,
                    a.Role,
                    Status = a.Status ?? "Active",
                    FirstName = a.Profile != null ? a.Profile.FirstName : "",
                    LastName = a.Profile != null ? a.Profile.LastName : "",
                    a.CanAccessITAssets,
                    a.CanAccessApparel,
                    Campuses = a.AccountCampuses.Select(ac => new
                    {
                        ac.CampusId,
                        Name = ac.Campus != null ? ac.Campus.Name : "Unknown",
                        ac.IsBlocked
                    }).ToList()
                })
                .ToListAsync();
            return Ok(users);
        }

        [HttpPost]
        public async Task<IActionResult> CreateUser([FromBody] CreateUserRequest request)
        {
            if (await _context.Accounts.AnyAsync(a => a.Username == request.Username))
                return BadRequest(new { message = "Username already exists" });

            if (await _context.Accounts.AnyAsync(a => a.Email == request.Email))
                return BadRequest(new { message = "Email already exists" });

            var account = new Account
            {
                Username = request.Username,
                Email = request.Email,
                Role = request.Role,
                PasswordHash = QuickStock.Infrastructure.Security.PasswordHelper.HashPassword(request.Password),
                Status = "Active",
                Verified = DateTime.UtcNow,
                CanAccessITAssets = request.CanAccessITAssets,
                CanAccessApparel = request.CanAccessApparel,
                Profile = new QuickStock.Domain.ITassets.Profile
                {
                    FirstName = request.FirstName,
                    LastName = request.LastName
                }
            };

            _context.Accounts.Add(account);
            await _context.SaveChangesAsync();

            return Ok(new { message = "User created successfully" });
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateUser(int id, [FromBody] UpdateUserRequest request)
        {
            var account = await _context.Accounts.Include(a => a.Profile).FirstOrDefaultAsync(a => a.Id == id);
            if (account == null) return NotFound();

            account.Email = request.Email;
            account.Role = request.Role;
            account.Username = request.Username;
            account.CanAccessITAssets = request.CanAccessITAssets;
            account.CanAccessApparel = request.CanAccessApparel;
            
            if (account.Profile != null)
            {
                account.Profile.FirstName = request.FirstName;
                account.Profile.LastName = request.LastName;
            }

            await _context.SaveChangesAsync();
            return Ok(new { message = "User updated successfully" });
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteUser(int id)
        {
            var account = await _context.Accounts.FindAsync(id);
            if (account == null) return NotFound();

            _context.Accounts.Remove(account);
            await _context.SaveChangesAsync();
            return Ok(new { message = "User deleted successfully" });
        }

        [HttpPut("{id}/toggle-status")]
        public async Task<IActionResult> ToggleUserStatus(int id)
        {
            var account = await _context.Accounts.FindAsync(id);
            if (account == null) return NotFound();

            account.Status = account.Status == "Active" ? "Disabled" : "Active";
            await _context.SaveChangesAsync();
            return Ok(new { status = account.Status });
        }

        [HttpPost("{userId}/campuses")]
        public async Task<IActionResult> AddCampusAccess(int userId, [FromBody] int campusId)
        {
            var exists = await _context.AccountCampuses
                .AnyAsync(ac => ac.AccountId == userId && ac.CampusId == campusId);
            
            if (exists) return Ok();

            _context.AccountCampuses.Add(new AccountCampus
            {
                AccountId = userId,
                CampusId = campusId
            });

            await _context.SaveChangesAsync();
            return Ok();
        }

        [HttpDelete("{userId}/campuses/{campusId}")]
        public async Task<IActionResult> RemoveCampusAccess(int userId, int campusId)
        {
            var mapping = await _context.AccountCampuses
                .FirstOrDefaultAsync(ac => ac.AccountId == userId && ac.CampusId == campusId);
            
            if (mapping == null) return NotFound();

            _context.AccountCampuses.Remove(mapping);
            await _context.SaveChangesAsync();
            return Ok();
        }

        [HttpPut("{userId}/campuses/{campusId}/toggle-block")]
        public async Task<IActionResult> ToggleBlock(int userId, int campusId)
        {
            var mapping = await _context.AccountCampuses
                .FirstOrDefaultAsync(ac => ac.AccountId == userId && ac.CampusId == campusId);
            
            if (mapping == null) return NotFound();

            mapping.IsBlocked = !mapping.IsBlocked;
            await _context.SaveChangesAsync();
            return Ok(new { isBlocked = mapping.IsBlocked });
        }
    }
}
