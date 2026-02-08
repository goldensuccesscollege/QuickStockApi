using MediatR;
using Microsoft.AspNetCore.WebUtilities;
using Microsoft.EntityFrameworkCore;
using QuickStock.Applications.Accounts.Command;
using QuickStock.Infrastructure.Data;
using QuickStock.Infrastructure.Services;
using System.Security.Cryptography;
using System.Threading;
using System.Threading.Tasks;

namespace QuickStock.Applications.Accounts.Handler
{
    public class ForgotPasswordHandler : IRequestHandler<ForgotPasswordCommand, string>
    {
        private readonly AppDbContext _db;
        private readonly EmailService _email;

        public ForgotPasswordHandler(AppDbContext db, EmailService email)
        {
            _db = db;
            _email = email;
        }

        public async Task<string> Handle(ForgotPasswordCommand request, CancellationToken cancellationToken)
        {
            var account = await _db.Accounts
                .FirstOrDefaultAsync(x => x.Email == request.Email, cancellationToken);

            if (account == null)
                return "If the email exists, a reset link has been sent."; // safe generic message

            // Generate a secure, URL-safe token
            var token = WebEncoders.Base64UrlEncode(RandomNumberGenerator.GetBytes(64));

            // Store token immediately in DB
            account.ResetToken = token;
            account.ResetTokenExpires = DateTime.UtcNow.AddHours(1); // valid for 1 hour
            account.Updated = DateTime.UtcNow;

            await _db.SaveChangesAsync(cancellationToken); // ✅ token saved here

            // Build the one-time use link
            string resetLink = $"https://localhost:7058/Account/ResetPassword?email={Uri.EscapeDataString(account.Email)}&token={Uri.EscapeDataString(token)}";

            // Send the email
            await _email.SendResetPasswordEmailAsync(account.Email, resetLink);

            return "If the email exists, a reset link has been sent.";
        }
    }
}
