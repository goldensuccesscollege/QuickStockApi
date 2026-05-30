using QuickStock.CQRS;
using Microsoft.EntityFrameworkCore;
using QuickStock.Applications.Apparel.Command;
using QuickStock.Infrastructure.Data;
using QuickStock.Domain.Apparel;
using QuickStock.Domain.ITassets;
using QuickStock.Domain.Locations;
using QuickStock.Domain.Shared;
using QuickStock.Domain.Accounts;
using System;
using System.Security.Claims;
using System.Threading;
using System.Threading.Tasks;

namespace QuickStock.Applications.Apparel.Handler
{
    public class UpdateItemStatusHandler : IRequestHandler<UpdateItemStatusCommand, bool>
    {
        private readonly AppDbContext _context;

        public UpdateItemStatusHandler(AppDbContext context)
        {
            _context = context;
        }

        public async Task<bool> Handle(UpdateItemStatusCommand request, CancellationToken cancellationToken)
        {
            var item = await _context.ApparelItems.Include(i => i.ApparelType).FirstOrDefaultAsync(i => i.Id == request.ItemId, cancellationToken);
            if (item == null) return false;

            var oldStatus = item.Status;
            item.Status = request.Status;
            item.LastModified = DateTime.UtcNow;

            // Update availability count
            if (item.ApparelType != null)
            {
                if (request.Status.Equals("Sold", StringComparison.OrdinalIgnoreCase) && !oldStatus.Equals("Sold", StringComparison.OrdinalIgnoreCase))
                {
                    item.ApparelType.Quality_In_Stock--;
                }
                else if (!request.Status.Equals("Sold", StringComparison.OrdinalIgnoreCase) && oldStatus.Equals("Sold", StringComparison.OrdinalIgnoreCase))
                {
                    item.ApparelType.Quality_In_Stock++;
                }
            }

            await _context.SaveChangesAsync(cancellationToken);

            if (request.Status.Equals("Sold", StringComparison.OrdinalIgnoreCase))
            {
                var apparel = item.ApparelType;
                await LogAction(request.User, "sold", item.Id, item.Apparel_Number, 
                    $"Apparel Name: {apparel?.Apparel_Name}, Apparel Number: {item.Apparel_Number}, Category: {apparel?.Category}, Sex: {apparel?.Sex}, Size: {apparel?.Size}, Grade Level: {apparel?.Grade_Level}, Unit Price: {apparel?.Unit_Price:C}", 
                    item.CampusId, item.Status);
            }
            else
            {
                await LogAction(request.User, "UpdateStatus", item.Id, item.Apparel_Number, $"Changed status from {oldStatus} to {request.Status}", item.CampusId, item.Status);
            }

            return true;
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
