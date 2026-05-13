using MediatR;
using Microsoft.EntityFrameworkCore;
using QuickStock.Applications.Apparel.Command;
using QuickStock.Infrastructure.Data;
using QuickStock.Domain.Apparel;
using QuickStock.Domain.ITassets;
using QuickStock.Domain.Messaging;
using QuickStock.Domain.Social;
using QuickStock.Domain.Locations;
using QuickStock.Domain.Shared;
using QuickStock.Domain.Accounts;
using QuickStock.Domain.Messaging;
using QuickStock.Domain.Social;
using QuickStock.Domain.Locations;
using QuickStock.Domain.Shared;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading;
using System.Threading.Tasks;

namespace QuickStock.Applications.Apparel.Handler
{
    public class AddStockHandler : IRequestHandler<AddStockCommand, object>
    {
        private readonly AppDbContext _context;

        public AddStockHandler(AppDbContext context)
        {
            _context = context;
        }

        public async Task<object> Handle(AddStockCommand request, CancellationToken cancellationToken)
        {
            using var transaction = await _context.Database.BeginTransactionAsync(cancellationToken);
            try
            {
                var apparel = await _context.ApparelList.FindAsync(new object[] { request.Id }, cancellationToken);
                if (apparel == null) throw new KeyNotFoundException("Apparel not found.");

                // Get current item count to determine next sequence number
                var currentItemsCount = await _context.ApparelItems
                    .Where(i => i.AppareldataId == request.Id)
                    .CountAsync(cancellationToken);

                var newItems = new List<ApparelItem>();
                for (int i = 1; i <= request.AdditionalQuantity; i++)
                {
                    newItems.Add(new ApparelItem
                    {
                        ApparelType = apparel,
                        Apparel_Number = $"AP-{apparel.Apparel_ID}-{DateTime.Now.Year}-{(currentItemsCount + i):D4}",
                        Status = "In Stock",
                        CampusId = apparel.CampusId,
                        DateCreated = DateTime.UtcNow,
                        LastModified = DateTime.UtcNow
                    });
                }

                _context.ApparelItems.AddRange(newItems);
                apparel.Quality_In_Stock += request.AdditionalQuantity;
                
                await _context.SaveChangesAsync(cancellationToken);
                await transaction.CommitAsync(cancellationToken);

                await LogAction(request.User, "Add Stock", apparel.Apparel_ID, apparel.Apparel_Name, $"Added {request.AdditionalQuantity} more items", apparel.CampusId, "In Stock");

                return new { success = true, newQuantity = apparel.Quality_In_Stock };
            }
            catch (Exception)
            {
                await transaction.RollbackAsync(cancellationToken);
                throw;
            }
        }

        private async Task LogAction(ClaimsPrincipal user, string action, int entityId, string entityName, string details, int campusId, string? status)
        {
            var userId = user.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            var username = user.Identity?.Name;
            
            var log = new AuditLog
            {
                Action = action,
                EntityType = "Apparel",
                EntityId = entityId,
                EntityName = entityName,
                Details = details,
                UserId = userId,
                Username = username,
                CampusId = campusId,
                Status = status
            };
            _context.AuditLogs.Add(log);
            await _context.SaveChangesAsync();
        }
    }
}
