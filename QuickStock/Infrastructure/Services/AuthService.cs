using Microsoft.EntityFrameworkCore;
using QuickStock.Applications.Accounts.Dto_s;
using QuickStock.Infrastructure.Data;
using System.Security.Cryptography;
using Microsoft.AspNetCore.WebUtilities;
using System.Threading.Tasks;

namespace QuickStock.Infrastructure.Services
{
    public class AuthService : IAuthService
    {
        private readonly AppDbContext _db;
        private readonly EmailService _emailService;

        public AuthService(AppDbContext db, EmailService emailService)
        {
            _db = db;
            _emailService = emailService;
        }

        public async Task<string> GenerateForgotPasswordTokenAsync(string email)
        {
            var account = await _db.Accounts.FirstOrDefaultAsync(u => u.Email == email);

            if (account == null)
                return "If the email exists, a reset link has been sent."; // generic safe message

            // Generate URL-safe token
            var token = WebEncoders.Base64UrlEncode(RandomNumberGenerator.GetBytes(64));
            account.ResetToken = token;
            account.ResetTokenExpires = DateTime.UtcNow.AddHours(1);

            await _db.SaveChangesAsync();

            string resetLink = $"https://localhost:7058/Account/ResetPassword?email={account.Email}&token={token}";

            // Send email
            await _emailService.SendResetPasswordEmailAsync(account.Email, resetLink);

            return "If the email exists, a reset link has been sent.";
        }

        public async Task ResetPasswordAsync(ResetPass request)
        {
            var account = await _db.Accounts.FirstOrDefaultAsync(u => u.Email == request.Email);

            if (account == null)
                throw new Exception("Invalid email address.");

            if (account.ResetToken != request.Token || account.ResetTokenExpires < DateTime.UtcNow)
                throw new Exception("Invalid or expired password reset token.");

            account.PasswordHash = BCrypt.Net.BCrypt.HashPassword(request.NewPassword);

            account.ResetToken = null;
            account.ResetTokenExpires = null;
            account.PasswordReset = DateTime.UtcNow;

            await _db.SaveChangesAsync();
        }
    }
}
