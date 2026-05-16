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
    public class GetConsumableItemsHandler : IRequestHandler<GetConsumableItemsQuery, List<ConsumableItemResponseDto>>
    {
        private readonly AppDbContext _context;
        public GetConsumableItemsHandler(AppDbContext context) { _context = context; }
        public async Task<List<ConsumableItemResponseDto>> Handle(GetConsumableItemsQuery request, CancellationToken cancellationToken)
        {
            var query = _context.ConsumableItems.Where(i => i.ConsumableDataId == request.ConsumableId);

            if (request.ShowOutOnly)
            {
                query = query.Where(i => i.Status == "Out");
            }
            else
            {
                query = query.Where(i => i.Status != "Out");
            }

            var items = await query.ToListAsync(cancellationToken);

            // Collect all potential identifiers for stockers
            var userIds = items.Where(i => !string.IsNullOrEmpty(i.AddedByUserId))
                               .Select(i => i.AddedByUserId!)
                               .Distinct().ToList();
            var usernames = items.Where(i => !string.IsNullOrEmpty(i.AddedByUsername))
                                 .Select(i => i.AddedByUsername!)
                                 .Distinct().ToList();

            // Also check audit logs to see who registered this consumable originally (for older items)
            var registrationLog = await _context.AuditLogs
                .Where(l => l.EntityType == "Consumable" && l.EntityId == request.ConsumableId && (l.Action == "Register" || l.Action == "Add"))
                .OrderBy(l => l.Timestamp)
                .FirstOrDefaultAsync(cancellationToken);
            
            if (registrationLog != null && !string.IsNullOrEmpty(registrationLog.UserId))
            {
                if (!userIds.Contains(registrationLog.UserId)) userIds.Add(registrationLog.UserId);
                if (!usernames.Contains(registrationLog.UserId)) usernames.Add(registrationLog.UserId);
            }

            // Fetch accounts and profiles in one go for better performance and reliability
            var intUserIds = userIds.Select(id => int.TryParse(id, out int aid) ? aid : 0).Where(id => id > 0).ToList();
            var profilesByAccount = await _context.Accounts
                .Include(a => a.Profile)
                .Where(a => intUserIds.Contains(a.Id) || usernames.Contains(a.Username))
                .ToListAsync(cancellationToken);

            var itemIds = items.Select(i => i.Id).ToList();
            var allRequests = await _context.ConsumableOutRequests
                .Where(r => itemIds.Contains(r.ConsumableItemId))
                .OrderByDescending(r => r.RequestedAt)
                .ToListAsync(cancellationToken);

            return items.Select(item => {
                var latestRequest = allRequests.FirstOrDefault(r => r.ConsumableItemId == item.Id);
                var isPending = latestRequest != null && latestRequest.Status == "Pending";
                var isApproved = latestRequest != null && latestRequest.Status == "Approved";

                // Resolve the display name
                string? displayStocker = null;
                
                // 1. Try to find by AccountId (AddedByUserId)
                if (int.TryParse(item.AddedByUserId, out int aid))
                {
                    displayStocker = profilesByAccount.FirstOrDefault(a => a.Id == aid)?.Profile?.FirstName;
                }
                
                // 2. Try to find by Username (AddedByUsername)
                if (string.IsNullOrEmpty(displayStocker) && !string.IsNullOrEmpty(item.AddedByUsername))
                {
                    displayStocker = profilesByAccount.FirstOrDefault(a => a.Username == item.AddedByUsername)?.Profile?.FirstName;
                }

                // 3. Try to find from the original Registration Log (for older items)
                if (string.IsNullOrEmpty(displayStocker) && registrationLog != null)
                {
                    var logUser = registrationLog.UserId;
                    displayStocker = profilesByAccount.FirstOrDefault(a => a.Username == logUser || a.Id.ToString() == logUser)?.Profile?.FirstName;
                }

                // 4. Fallback to stored AddedByUsername or "System"
                displayStocker ??= item.AddedByUsername;

                return new ConsumableItemResponseDto
                {
                    Id = item.Id,
                    ItemCode = item.ItemCode,
                    Status = item.Status,
                    DateOut = item.DateOut,
                    ConsumableDataId = item.ConsumableDataId,
                    AddedByUsername = displayStocker,
                    IsRequestPending = isPending,
                    PendingRequestId = isPending ? latestRequest?.Id : null,
                    RequestedByUserId = latestRequest?.RequestedByUserId,
                    RequestedByUsername = latestRequest?.RequestedByUsername,
                    RequestedAt = latestRequest?.RequestedAt,
                    ApprovedByUsername = isApproved ? latestRequest?.ApprovedByUsername : null,
                    ApprovedAt = isApproved ? latestRequest?.ApprovedAt : null
                };
            }).ToList();
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

            string action;
            string details;

            if (request.Status == "Out")
            {
                item.DateOut = DateTime.UtcNow;
                item.ConsumableData.Out++;
                action = "Inventory Out";
                details = $"Item '{item.ItemCode}' of product '{item.ConsumableData.Product}' marked as Out. " +
                          $"New balance: {item.ConsumableData.In - item.ConsumableData.Out}.";
            }
            else if (oldStatus == "Out" && request.Status == "In Stock")
            {
                item.DateOut = null;
                item.ConsumableData.Out--;
                action = "Returned to Stock";
                details = $"Item '{item.ItemCode}' of product '{item.ConsumableData.Product}' returned to In Stock. " +
                          $"New balance: {item.ConsumableData.In - item.ConsumableData.Out}.";
            }
            else
            {
                action = "Update Status";
                details = $"Item '{item.ItemCode}' status changed from '{oldStatus}' to '{request.Status}'.";
            }

            await _context.SaveChangesAsync(cancellationToken);

            // Log the action under EntityType "Consumable" so it appears in the Consumable Activity Logs
            var userId = request.User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            _context.AuditLogs.Add(new AuditLog
            {
                Action = action,
                EntityType = "Consumable",
                EntityId = item.ConsumableData.Id,
                EntityName = item.ConsumableData.Product,
                Details = details,
                UserId = userId,
                Username = request.User.Identity?.Name,
                CampusId = item.ConsumableData.CampusId,
                Status = request.Status,
                Timestamp = DateTime.UtcNow
            });
            await _context.SaveChangesAsync(cancellationToken);

            return true;
        }
    }
}
