using MediatR;
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
using System.Security.Claims;
using System.Threading;
using System.Threading.Tasks;

namespace QuickStock.Applications.Itassets.Handler
{
    public class DeleteItAssetHandler : IRequestHandler<DeleteItAssetCommand, bool>
    {
        private readonly AppDbContext _context;
        private readonly QuickStock.Infrastructure.Services.INotificationService _notificationService;

        public DeleteItAssetHandler(AppDbContext context, QuickStock.Infrastructure.Services.INotificationService notificationService)
        {
            _context = context;
            _notificationService = notificationService;
        }

        public async Task<bool> Handle(DeleteItAssetCommand request, CancellationToken cancellationToken)
        {
            var asset = await _context.Itassets.FindAsync(new object[] { request.Id }, cancellationToken);
            if (asset == null) return false;

            var assetName = asset.Name;
            var campusId = asset.CampusId;
            var assetStatus = asset.Status;

            _context.Itassets.Remove(asset);
            await _context.SaveChangesAsync(cancellationToken);

            await LogAction(request.User, "Delete", request.Id, assetName, $"Deleted IT Asset: {assetName}", campusId, assetStatus);
            
            await _notificationService.NotifyCampusActivity(campusId, "IT Asset Removed", 
                $"{request.User.Identity?.Name} removed {assetName} from inventory", "Danger");

            return true;
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
