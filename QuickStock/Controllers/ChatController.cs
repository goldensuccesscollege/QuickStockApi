using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using QuickStock.Infrastructure.Data;

namespace QuickStock.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class ChatController : ControllerBase
    {
        private readonly AppDbContext _context;

        public ChatController(AppDbContext context)
        {
            _context = context;
        }

        [HttpGet("history")]
        public async Task<IActionResult> GetHistory([FromQuery] string? withUser = null, [FromQuery] int? groupId = null, [FromQuery] int limit = 50)
        {
            var currentUsername = User.Identity?.Name;
            var query = _context.ChatMessages
                .Include(m => m.Sender)
                .ThenInclude(s => s.Profile)
                .Include(m => m.Receiver)
                .ThenInclude(r => r.Profile)
                .AsQueryable();

            if (groupId.HasValue)
            {
                // Group chat history
                query = query.Where(m => m.GroupId == groupId.Value);
            }
            else if (string.IsNullOrEmpty(withUser))
            {
                // Global chat history
                query = query.Where(m => m.ReceiverAccountId == null && m.GroupId == null);
            }
            else
            {
                // Private chat history between current user and 'withUser'
                query = query.Where(m => 
                    (m.Sender.Username == currentUsername && m.Receiver != null && m.Receiver.Username == withUser) ||
                    (m.Sender.Username == withUser && m.Receiver != null && m.Receiver.Username == currentUsername));
            }

            var messages = await query
                .OrderByDescending(m => m.Timestamp)
                .Take(limit)
                .Select(m => new
                {
                    Username = m.Sender.Username,
                    FullName = m.Sender.Profile != null ? $"{m.Sender.Profile.FirstName} {m.Sender.Profile.LastName}" : m.Sender.Username,
                    ImageProfilePath = m.Sender.Profile != null ? m.Sender.Profile.ImageProfilePath : null,
                    Message = m.Message,
                    Timestamp = m.Timestamp.ToString("yyyy-MM-dd HH:mm:ss"),
                    IsPrivate = m.ReceiverAccountId != null,
                    GroupId = m.GroupId
                })
                .ToListAsync();

            messages.Reverse();
            return Ok(messages);
        }

        [HttpGet("users")]
        public async Task<IActionResult> GetUsers()
        {
            var currentUsername = User.Identity?.Name;
            var users = await _context.Accounts
                .Include(a => a.Profile)
                .Where(a => a.Username != currentUsername)
                .Select(a => new { 
                    username = a.Username,
                    fullName = a.Profile != null ? $"{a.Profile.FirstName} {a.Profile.LastName}" : a.Username,
                    firstName = a.Profile != null ? a.Profile.FirstName : "",
                    lastName = a.Profile != null ? a.Profile.LastName : "",
                    imageProfilePath = a.Profile != null ? a.Profile.ImageProfilePath : null
                })
                .ToListAsync();
            
            return Ok(users);
        }

        [HttpGet("groups")]
        public async Task<IActionResult> GetGroups()
        {
            var currentUsername = User.Identity?.Name;
            var groups = await _context.ChatGroupMembers
                .Include(gm => gm.Group)
                .Where(gm => gm.Account.Username == currentUsername)
                .Select(gm => new {
                    id = gm.Group.Id,
                    name = gm.Group.Name,
                })
                .ToListAsync();

            return Ok(groups);
        }

        [HttpPost("groups")]
        public async Task<IActionResult> CreateGroup([FromBody] CreateGroupDto dto)
        {
            var currentUsername = User.Identity?.Name;
            var creator = await _context.Accounts.FirstOrDefaultAsync(a => a.Username == currentUsername);
            if (creator == null) return Unauthorized();

            var group = new Domain.ITassets.ChatGroup
            {
                Name = dto.Name,
                CreatedByAccountId = creator.Id,
                CreatedAt = DateTime.UtcNow
            };

            _context.ChatGroups.Add(group);
            await _context.SaveChangesAsync();

            // Add members
            var memberUsernames = dto.Usernames.Distinct(StringComparer.OrdinalIgnoreCase).ToList();
            if (!memberUsernames.Contains(currentUsername!, StringComparer.OrdinalIgnoreCase))
            {
                memberUsernames.Add(currentUsername!);
            }

            var members = await _context.Accounts
                .Where(a => memberUsernames.Contains(a.Username))
                .Select(a => new Domain.ITassets.ChatGroupMember
                {
                    ChatGroupId = group.Id,
                    AccountId = a.Id
                })
                .ToListAsync();

            _context.ChatGroupMembers.AddRange(members);
            await _context.SaveChangesAsync();

            return Ok(new { id = group.Id, name = group.Name });
        }
    }
}
