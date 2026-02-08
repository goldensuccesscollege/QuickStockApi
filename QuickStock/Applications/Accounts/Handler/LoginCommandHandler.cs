using MediatR;
using Microsoft.EntityFrameworkCore;
using QuickStock.Applications.Accounts.Command;
using QuickStock.Applications.Accounts.Dto_s;
using QuickStock.Common.Exceptions;
using QuickStock.Infrastructure.Data;
using QuickStock.Infrastructure.Security;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace QuickStock.Applications.Accounts.Handler
{
    public class LoginCommandHandler
        : IRequestHandler<LoginCommand, LoginResponse>
    {
        private readonly AppDbContext _db;
        private readonly IConfiguration _config;

        public LoginCommandHandler(AppDbContext db, IConfiguration config)
        {
            _db = db;
            _config = config;
        }

        public async Task<LoginResponse> Handle(
            LoginCommand request,
            CancellationToken cancellationToken)
        {
            var user = await _db.Accounts
                .FirstOrDefaultAsync(
                    x => x.Username == request.Username,
                    cancellationToken);

            // ❌ Wrong credentials
            if (user == null)
                throw new UnauthorizedException("Invalid username or password.");

            // ❌ Not verified
            if (user.Verified == null)
                throw new UnauthorizedException("Account not verified.");

            // ❌ Inactive
            if (user.Status == "Inactive")
                throw new UnauthorizedException("This account is inactive. Please contact support.");

            // ❌ Wrong password
            if (!PasswordHelper.VerifyPassword(
                request.Password,
                user.PasswordHash))
                throw new UnauthorizedException("Invalid username or password.");

            // ✅ Claims
            var claims = new[]
            {
                new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
                new Claim(JwtRegisteredClaimNames.Sub, user.Email),
                new Claim(ClaimTypes.Role, user.Role),
                new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())
            };

            var key = new SymmetricSecurityKey(
                Encoding.UTF8.GetBytes(
                    _config["Jwt:Key"]
                    ?? throw new InvalidOperationException("JWT Key not configured"))
            );

            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

            var token = new JwtSecurityToken(
                issuer: _config["Jwt:Issuer"],
                audience: _config["Jwt:Audience"],
                claims: claims,
                expires: DateTime.UtcNow.AddHours(1),
                signingCredentials: creds
            );

            return new LoginResponse
            {
                Token = new JwtSecurityTokenHandler().WriteToken(token),
                Email = user.Email,
                Role = user.Role
            };
        }
    }
}
