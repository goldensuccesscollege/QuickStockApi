using MediatR;
using Microsoft.EntityFrameworkCore;
using QuickStock.Applications.Itassets.Command;
using QuickStock.Infrastructure.Data;
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
using System.Security.Claims;
using System.Threading;
using System.Threading.Tasks;

namespace QuickStock.Applications.Itassets.Handler
{
    public class CreateItAssetHandler : IRequestHandler<CreateItAssetCommand, ItAsset>
    {
        private readonly AppDbContext _context;
        private readonly QuickStock.Infrastructure.Services.INotificationService _notificationService;

        public CreateItAssetHandler(AppDbContext context, QuickStock.Infrastructure.Services.INotificationService notificationService)
        {
            _context = context;
            _notificationService = notificationService;
        }

        public async Task<ItAsset> Handle(CreateItAssetCommand request, CancellationToken cancellationToken)
        {
            var asset = request.Asset;

            if (!string.IsNullOrWhiteSpace(asset.SerialNumber))
            {
                var duplicate = await _context.Itassets.AnyAsync(
                    a => a.SerialNumber == asset.SerialNumber && a.CampusId == asset.CampusId,
                    cancellationToken);
                if (duplicate)
                    throw new InvalidOperationException("An item with this serial number already exists in this campus.");
            }

            if (string.IsNullOrWhiteSpace(asset.Qrcode))
            {
                asset.Qrcode = Guid.NewGuid().ToString("N").Substring(0, 12).ToUpper();
            }

            asset.DateAdded = DateTime.UtcNow;
            _context.Itassets.Add(asset);
            await _context.SaveChangesAsync(cancellationToken);

            await LogAction(request.User, "Add", asset.Id, asset.Name,
                $"Added new {asset.Type}: {asset.Name} (Auto-QR: {asset.Qrcode})", asset.CampusId, asset.Status);

            // Send Real-time Notification
            await _notificationService.NotifyCampusActivity(asset.CampusId, "New IT Asset Added", 
                $"{request.User.Identity?.Name} added {asset.Name} to {asset.Location}", "Success");

            return asset;
        }

        private async Task LogAction(ClaimsPrincipal user, string action, int entityId, string entityName, string details, int campusId, string? status)
        {
            var log = new AuditLog
            {
                Action = action,
                EntityType = "ItAsset",
                EntityId = entityId,
                EntityName = entityName,
                Details = details,
                UserId = user.FindFirst(ClaimTypes.NameIdentifier)?.Value,
                Username = user.Identity?.Name,
                CampusId = campusId,
                Status = status
            };
            _context.AuditLogs.Add(log);
            await _context.SaveChangesAsync();
        }
    }
}
