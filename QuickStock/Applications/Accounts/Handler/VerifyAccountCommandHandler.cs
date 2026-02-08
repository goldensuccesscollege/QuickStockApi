using MediatR;
using Microsoft.EntityFrameworkCore;
using QuickStock.Applications.Accounts.Command;
using QuickStock.Infrastructure.Data;

namespace QuickStock.Applications.Accounts.Handler
{
    public class VerifyAccountCommandHandler : IRequestHandler<VerifyAccountCommand, string>
    {
        private readonly AppDbContext _db;

        public VerifyAccountCommandHandler(AppDbContext db)
        {
            _db = db;
        }

        public async Task<string> Handle(VerifyAccountCommand request, CancellationToken cancellationToken)
        {
            var user = await _db.Accounts
                .FirstOrDefaultAsync(x => x.VerificationTokens == request.Token, cancellationToken);

            if (user == null)
                return "Invalid verification token.";

            user.Verified = DateTime.UtcNow;
            user.VerificationTokens = null;
            user.Status = "Active";

            await _db.SaveChangesAsync(cancellationToken);

            return "Account verified successfully. You can now log in.";
        }
    }
}
