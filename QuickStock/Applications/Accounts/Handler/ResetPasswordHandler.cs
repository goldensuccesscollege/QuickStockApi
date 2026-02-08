using MediatR;
using Microsoft.EntityFrameworkCore;
using QuickStock.Applications.Accounts.Command;
using QuickStock.Infrastructure.Data;
using System.Threading;
using System.Threading.Tasks;

namespace QuickStock.Applications.Accounts.Handler
{
    public class ResetPasswordHandler : IRequestHandler<ResetPasswordCommand, string>
    {
        private readonly AppDbContext _db;

        public ResetPasswordHandler(AppDbContext db)
        {
            _db = db;
        }

        public async Task<string> Handle(ResetPasswordCommand request, CancellationToken cancellationToken)
        {
            var account = await _db.Accounts
                .FirstOrDefaultAsync(x =>
                    x.Email == request.Email &&
                    x.ResetToken == request.Token,
                    cancellationToken);

            // ❌ Token not found
            if (account == null)
                return "Invalid reset link.";

            // ❌ Token expired
            if (account.ResetTokenExpires < DateTime.UtcNow)
                return "Reset link has expired.";

            // ✅ Hash new password
            account.PasswordHash = BCrypt.Net.BCrypt.HashPassword(request.NewPassword);

            // ✅ Invalidate token (IMPORTANT)
            account.ResetToken = null;
            account.ResetTokenExpires = null;
            account.PasswordReset = DateTime.UtcNow;
            account.Updated = DateTime.UtcNow;

            await _db.SaveChangesAsync(cancellationToken);

            return "Password reset successful.";
        }
    }
}
