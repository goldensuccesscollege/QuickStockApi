using MediatR;
using Microsoft.EntityFrameworkCore;
using QuickStock.Applications.Consumables.Queries;
using QuickStock.Applications.Consumables.Command;
using QuickStock.Infrastructure.Data;
using QuickStock.Domain.Consumables;
using QuickStock.Domain.Shared;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading;
using System.Threading.Tasks;

namespace QuickStock.Applications.Consumables.Handler
{
    public class GetConsumableItemsHandler : IRequestHandler<GetConsumableItemsQuery, List<ConsumableItem>>
    {
        private readonly AppDbContext _context;
        public GetConsumableItemsHandler(AppDbContext context) { _context = context; }
        public async Task<List<ConsumableItem>> Handle(GetConsumableItemsQuery request, CancellationToken cancellationToken)
        {
            return await _context.ConsumableItems
                .Where(i => i.ConsumableDataId == request.ConsumableId)
                .ToListAsync(cancellationToken);
        }
    }

    public class UpdateConsumableItemStatusHandler : IRequestHandler<UpdateConsumableItemStatusCommand, bool>
    {
        private readonly AppDbContext _context;
        public UpdateConsumableItemStatusHandler(AppDbContext context) { _context = context; }

        public async Task<bool> Handle(UpdateConsumableItemStatusCommand request, CancellationToken cancellationToken)
        {
            var item = await _context.ConsumableItems
                .Include(i => i.ConsumableData)
                .FirstOrDefaultAsync(i => i.Id == request.ItemId, cancellationToken);

            if (item == null) return false;

            var oldStatus = item.Status;
            item.Status = request.Status;
            
            if (request.Status == "Out")
            {
                item.DateOut = DateTime.UtcNow;
                item.ConsumableData.Out++;
            }
            else if (oldStatus == "Out" && request.Status == "In Stock")
            {
                item.DateOut = null;
                item.ConsumableData.Out--;
            }

            await _context.SaveChangesAsync(cancellationToken);

            // Log
            var userId = request.User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            _context.AuditLogs.Add(new AuditLog
            {
                Action = "Update Status",
                EntityType = "ConsumableItem",
                EntityId = item.Id,
                EntityName = item.ItemCode,
                Details = $"Status changed from {oldStatus} to {request.Status}",
                UserId = userId,
                Username = request.User.Identity?.Name,
                CampusId = item.ConsumableData.CampusId,
                Status = "Success",
                Timestamp = DateTime.UtcNow
            });
            await _context.SaveChangesAsync(cancellationToken);

            return true;
        }
    }
}
