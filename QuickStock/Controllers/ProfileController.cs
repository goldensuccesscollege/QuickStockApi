using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using QuickStock.Applications.Profile.DTO_s;
using QuickStock.Applications.Profile.Handler;
using Microsoft.EntityFrameworkCore;
using QuickStock.Infrastructure.Data;
using QuickStock.Domain.ITassets;
using QuickStock.Domain.Locations;
using QuickStock.Domain.Shared;
using QuickStock.Domain.Accounts;
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
    }
}
