using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using QuickStock.Applications.Profile.DTO_s;
using QuickStock.Applications.Profile.Handler;
using Microsoft.EntityFrameworkCore;
using QuickStock.Infrastructure.Data;
using QuickStock.Domain.ITassets;
using System.IO;
using System;
using System.Linq;

namespace QuickStock.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class ProfileController : ControllerBase
    {
        private readonly UpdateProfileHandler _updateHandler;
        private readonly GetProfileHandler _getHandler;
        private readonly AppDbContext _context;
        private readonly IWebHostEnvironment _env;

        public ProfileController(UpdateProfileHandler updateHandler, GetProfileHandler getHandler, AppDbContext context, IWebHostEnvironment env)
        {
            _updateHandler = updateHandler;
            _getHandler = getHandler;
            _context = context;
            _env = env;
        }

        [HttpGet("me")]
        public async Task<IActionResult> GetProfile()
        {
            var accountId = int.Parse(User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value ?? "0");
            var profile = await _getHandler.Handle(accountId);
            return Ok(profile);
        }

        [HttpPut("update")]
        public async Task<IActionResult> UpdateProfile([FromForm] UpdateProfileDto dto)
        {
            var accountId = int.Parse(User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value ?? "0");
            var result = await _updateHandler.Handle(accountId, dto);

            return Ok(new { message = result });
        }

        [HttpGet("user/{username}")]
        public async Task<IActionResult> GetPublicProfile(string username)
        {
            var account = await _getHandler.GetByUsername(username);
            if (account == null) return NotFound(new { message = "User not found" });

            return Ok(account);
        }

        [HttpPost("posts")]
        public async Task<IActionResult> CreatePost([FromForm] CreatePostDto dto)
        {
            var accountId = int.Parse(User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value ?? "0");
            
            if (string.IsNullOrWhiteSpace(dto.Content) && (dto.Images == null || dto.Images.Count == 0))
            {
                return BadRequest(new { message = "Post must have content or at least one image." });
            }

            var post = new ProfilePost
            {
                AuthorAccountId = accountId,
                Content = dto.Content ?? "",
                CreatedAt = DateTime.UtcNow
            };

            if (dto.Images != null && dto.Images.Count > 0)
            {
                var uploadDir = Path.Combine(_env.WebRootPath, "post_images");
                if (!Directory.Exists(uploadDir)) Directory.CreateDirectory(uploadDir);

                foreach (var file in dto.Images)
                {
                    var fileName = $"{Guid.NewGuid()}{Path.GetExtension(file.FileName)}";
                    var filePath = Path.Combine(uploadDir, fileName);
                    using (var stream = new FileStream(filePath, FileMode.Create))
                    {
                        await file.CopyToAsync(stream);
                    }
                    post.Images.Add(new PostImage { ImagePath = $"/post_images/{fileName}" });
                }
            }

            _context.ProfilePosts.Add(post);
            await _context.SaveChangesAsync();

            return Ok(new { message = "Post created successfully" });
        }

        [HttpGet("posts/{username}")]
        public async Task<IActionResult> GetPosts(string username)
        {
            var currentUserAccount = await _context.Accounts.FirstOrDefaultAsync(a => a.Username == User.Identity!.Name);
            var currentUserId = currentUserAccount?.Id ?? 0;

            var posts = await _context.ProfilePosts
                .Include(p => p.Author).ThenInclude(a => a.Profile)
                .Include(p => p.Images)
                .Include(p => p.Reactions)
                .Include(p => p.Comments).ThenInclude(c => c.Author).ThenInclude(a => a.Profile)
                .Where(p => p.Author.Username == username)
                .OrderByDescending(p => p.CreatedAt)
                .Select(p => new PostResponseDto
                {
                    Id = p.Id,
                    AuthorUsername = p.Author.Username,
                    AuthorFullName = $"{p.Author.Profile.FirstName} {p.Author.Profile.LastName}",
                    AuthorProfileImage = p.Author.Profile.ImageProfilePath ?? "",
                    Content = p.Content,
                    CreatedAt = p.CreatedAt,
                    Images = p.Images.Select(i => i.ImagePath).ToList(),
                    ReactionsCount = p.Reactions.Count,
                    CommentsCount = p.Comments.Count,
                    IsLikedByCurrentUser = p.Reactions.Any(r => r.AuthorAccountId == currentUserId),
                    Comments = p.Comments.OrderBy(c => c.CreatedAt).Select(c => new CommentDto
                    {
                        Id = c.Id,
                        AuthorUsername = c.Author.Username,
                        AuthorFullName = $"{c.Author.Profile.FirstName} {c.Author.Profile.LastName}",
                        AuthorProfileImage = c.Author.Profile.ImageProfilePath ?? "",
                        Content = c.Content,
                        CreatedAt = c.CreatedAt
                    }).ToList()
                })
                .ToListAsync();

            return Ok(posts);
        }

        [HttpPost("posts/{postId}/react")]
        public async Task<IActionResult> ToggleReaction(int postId)
        {
            var accountId = int.Parse(User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value ?? "0");
            var reaction = await _context.PostReactions
                .FirstOrDefaultAsync(r => r.PostId == postId && r.AuthorAccountId == accountId);

            if (reaction != null)
            {
                _context.PostReactions.Remove(reaction);
                await _context.SaveChangesAsync();
                return Ok(new { liked = false });
            }
            else
            {
                _context.PostReactions.Add(new PostReaction
                {
                    PostId = postId,
                    AuthorAccountId = accountId,
                    Type = "Like"
                });
                await _context.SaveChangesAsync();
                return Ok(new { liked = true });
            }
        }

        [HttpPost("posts/{postId}/comments")]
        public async Task<IActionResult> AddComment(int postId, [FromForm] string content)
        {
            var accountId = int.Parse(User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value ?? "0");
            
            if (string.IsNullOrWhiteSpace(content))
                return BadRequest(new { message = "Comment cannot be empty." });

            var comment = new PostComment
            {
                PostId = postId,
                AuthorAccountId = accountId,
                Content = content,
                CreatedAt = DateTime.UtcNow
            };

            _context.PostComments.Add(comment);
            await _context.SaveChangesAsync();

            // Return the comment info for UI update
            var account = await _context.Accounts.Include(a => a.Profile).FirstAsync(a => a.Id == accountId);
            return Ok(new CommentDto
            {
                Id = comment.Id,
                AuthorUsername = account.Username,
                AuthorFullName = $"{account.Profile.FirstName} {account.Profile.LastName}",
                AuthorProfileImage = account.Profile.ImageProfilePath ?? "",
                Content = comment.Content,
                CreatedAt = comment.CreatedAt
            });
        }

        [HttpDelete("posts/{postId}")]
        public async Task<IActionResult> DeletePost(int postId)
        {
            var accountId = int.Parse(User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value ?? "0");
            var post = await _context.ProfilePosts.FindAsync(postId);
            
            if (post == null) return NotFound();
            if (post.AuthorAccountId != accountId) return Unauthorized();

            _context.ProfilePosts.Remove(post);
            await _context.SaveChangesAsync();

            return Ok(new { message = "Post deleted" });
        }

        [HttpGet("photos/{username}")]
        public async Task<IActionResult> GetPhotos(string username)
        {
            var photos = await _context.PostImages
                .Include(i => i.Post).ThenInclude(p => p.Author)
                .Where(i => i.Post.Author.Username == username)
                .OrderByDescending(i => i.Post.CreatedAt)
                .Select(i => i.ImagePath)
                .ToListAsync();

            return Ok(photos);
        }
    }
}
