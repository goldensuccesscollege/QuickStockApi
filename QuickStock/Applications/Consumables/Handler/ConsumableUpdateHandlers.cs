using MediatR;
using Microsoft.EntityFrameworkCore;
using QuickStock.Applications.Consumables.Command;
using QuickStock.Domain.Consumables;
using QuickStock.Infrastructure.Data;
using System.Security.Claims;

namespace QuickStock.Applications.Consumables.Handler
{
    public class UpdateConsumableHandler : IRequestHandler<UpdateConsumableCommand, bool>
    {
        private readonly AppDbContext _context;
        public UpdateConsumableHandler(AppDbContext context) { _context = context; }

        public async Task<bool> Handle(UpdateConsumableCommand request, CancellationToken cancellationToken)
        {
            var existing = await _context.ConsumableList
                .FirstOrDefaultAsync(c => c.Id == request.Consumable.Id, cancellationToken);

            if (existing == null) return false;

            existing.Product = request.Consumable.Product;
            existing.Description = request.Consumable.Description;
            existing.Unit = request.Consumable.Unit;
            existing.DateArrived = request.Consumable.DateArrived;

            await _context.SaveChangesAsync(cancellationToken);
            return true;
        }
    }

    public class RestockConsumableHandler : IRequestHandler<RestockConsumableCommand, bool>
    {
        private readonly AppDbContext _context;
        public RestockConsumableHandler(AppDbContext context) { _context = context; }

        public async Task<bool> Handle(RestockConsumableCommand request, CancellationToken cancellationToken)
        {
            var consumable = await _context.ConsumableList
                .FirstOrDefaultAsync(c => c.Id == request.ConsumableId, cancellationToken);

            if (consumable == null || request.QuantityToAdd <= 0) return false;

            // Increment In count
            consumable.In += request.QuantityToAdd;

            var userId = request.User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            var accountId = int.TryParse(userId, out int aid) ? aid : 0;
            var profile = await _context.Profiles.FirstOrDefaultAsync(p => p.AccountId == accountId, cancellationToken);
            var stockerName = profile != null ? profile.FirstName : request.User.Identity?.Name;

            // Create new individual items
            for (int i = 0; i < request.QuantityToAdd; i++)
            {
                var item = new ConsumableItem
                {
                    ConsumableDataId = consumable.Id,
                    ItemCode = $"{consumable.Product.Replace(" ", "").ToUpper()}-{Guid.NewGuid().ToString().Substring(0, 4)}",
                    Status = "In Stock",
                    AddedByUserId = userId,
                    AddedByUsername = stockerName
                };
                _context.ConsumableItems.Add(item);
            }

            await _context.SaveChangesAsync(cancellationToken);
            return true;
        }
    }
}
