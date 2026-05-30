using QuickStock.CQRS;
using QuickStock.Applications.Users.Command;
using QuickStock.Infrastructure.Data;
using System.Threading;
using System.Threading.Tasks;

namespace QuickStock.Applications.Users.Handler
{
    public class ToggleConsumablesAccessHandler : IRequestHandler<ToggleConsumablesAccessCommand, object>
    {
        private readonly AppDbContext _context;

        public ToggleConsumablesAccessHandler(AppDbContext context)
        {
            _context = context;
        }

        public async Task<object> Handle(ToggleConsumablesAccessCommand request, CancellationToken cancellationToken)
        {
            var user = await _context.Accounts.FindAsync(request.Id);
            if (user == null) throw new KeyNotFoundException("User not found");

            user.CanAccessConsumables = !user.CanAccessConsumables;
            await _context.SaveChangesAsync(cancellationToken);

            return new { success = true, message = $"Consumables access {(user.CanAccessConsumables ? "enabled" : "disabled")}" };
        }
    }
}
