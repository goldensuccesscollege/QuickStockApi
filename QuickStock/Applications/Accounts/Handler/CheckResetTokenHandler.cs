using MediatR;
using Microsoft.EntityFrameworkCore;
using QuickStock.Applications.Accounts.Queries;
using QuickStock.Infrastructure.Data;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace QuickStock.Applications.Accounts.Handler
{
    public class CheckResetTokenHandler : IRequestHandler<CheckResetTokenQuery, bool>
    {
        private readonly AppDbContext _db;

        public CheckResetTokenHandler(AppDbContext db)
        {
            _db = db;
        }

        public async Task<bool> Handle(CheckResetTokenQuery request, CancellationToken cancellationToken)
        {
            var account = await _db.Accounts.FirstOrDefaultAsync(a => a.Email == request.Email, cancellationToken);

            if (account == null) return false;
            if (account.ResetToken != request.Token) return false;
            if (account.ResetTokenExpires < DateTime.UtcNow) return false;

            return true;
        }
    }
}
