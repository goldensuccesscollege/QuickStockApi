using MediatR;
using Microsoft.EntityFrameworkCore;
using QuickStock.Applications.Consumables.Command;
using QuickStock.Infrastructure.Data;
using QuickStock.Domain.Consumables;
using QuickStock.Domain.Shared;
using System;
using System.Collections.Generic;
using System.Security.Claims;
using System.Threading;
using System.Threading.Tasks;

namespace QuickStock.Applications.Consumables.Handler
{
    public class CreateConsumableHandler : IRequestHandler<CreateConsumableCommand, ConsumableData>
    {
        private readonly AppDbContext _context;

        public CreateConsumableHandler(AppDbContext context)
        {
            _context = context;
        }

        public async Task<ConsumableData> Handle(CreateConsumableCommand request, CancellationToken cancellationToken)
        {
            var consumable = request.Consumable;
            using var transaction = await _context.Database.BeginTransactionAsync(cancellationToken);
            try
            {
                _context.ConsumableList.Add(consumable);
                await _context.SaveChangesAsync(cancellationToken);

                // Automatically generate individual items based on 'In' quantity
                var userId = request.User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                var accountId = int.TryParse(userId, out int aid) ? aid : 0;
                var profile = await _context.Profiles.FirstOrDefaultAsync(p => p.AccountId == accountId, cancellationToken);
                var stockerName = profile != null ? profile.FirstName : request.User.Identity?.Name;

                var items = new List<ConsumableItem>();
                for (int i = 1; i <= consumable.In; i++)
                {
                    items.Add(new ConsumableItem
                    {
                        ConsumableData = consumable,
                        ItemCode = $"{consumable.Product.Substring(0, Math.Min(3, consumable.Product.Length)).ToUpper()}-{Guid.NewGuid().ToString("N").Substring(0, 6).ToUpper()}-{i:D3}",
                        Status = "In Stock",
                        AddedByUserId = userId,
                        AddedByUsername = stockerName
                    });
                }
                _context.ConsumableItems.AddRange(items);
                await _context.SaveChangesAsync(cancellationToken);

                await transaction.CommitAsync(cancellationToken);

                await LogAction(request.User, "Register", consumable.Id, consumable.Product, 
                    $"Registered new consumable: {consumable.Product} (Qty: {consumable.In})", consumable.CampusId);

                return consumable;
            }
            catch (Exception)
            {
                await transaction.RollbackAsync(cancellationToken);
                throw;
            }
        }

        private async Task LogAction(ClaimsPrincipal user, string action, int entityId, string entityName, string details, int campusId)
        {
            var userId = user.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            var username = user.Identity?.Name;
            
            var log = new AuditLog
            {
                Action = action,
                EntityType = "Consumable",
                EntityId = entityId,
                EntityName = entityName,
                Details = details,
                UserId = userId,
                Username = username,
                CampusId = campusId,
                Status = "Success",
                Timestamp = DateTime.UtcNow
            };
            _context.AuditLogs.Add(log);
            await _context.SaveChangesAsync();
        }
    }
}
