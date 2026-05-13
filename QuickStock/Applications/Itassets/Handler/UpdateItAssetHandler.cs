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
    public class UpdateItAssetHandler : IRequestHandler<UpdateItAssetCommand, bool>
    {
        private readonly AppDbContext _context;

        public UpdateItAssetHandler(AppDbContext context)
        {
            _context = context;
        }

        public async Task<bool> Handle(UpdateItAssetCommand request, CancellationToken cancellationToken)
        {
            var asset = request.Asset;

            if (!string.IsNullOrWhiteSpace(asset.SerialNumber))
            {
                var duplicate = await _context.Itassets.AnyAsync(
                    a => a.SerialNumber == asset.SerialNumber && a.CampusId == asset.CampusId && a.Id != request.Id,
                    cancellationToken);
                if (duplicate)
                    throw new InvalidOperationException("An item with this serial number already exists in this campus.");
            }

            var existingAsset = await _context.Itassets.FindAsync(new object[] { request.Id }, cancellationToken);
            if (existingAsset == null) return false;

            existingAsset.Name = asset.Name;
            existingAsset.Type = asset.Type;
            existingAsset.Brand = asset.Brand;
            existingAsset.Model = asset.Model;
            existingAsset.Location = asset.Location;
            existingAsset.Status = asset.Status;
            existingAsset.Qrcode = asset.Qrcode;
            existingAsset.RoomId = asset.RoomId;
            existingAsset.SerialNumber = asset.SerialNumber;

            await _context.SaveChangesAsync(cancellationToken);
            await LogAction(request.User, "Update", asset.Id, asset.Name, "Updated asset details", asset.CampusId, asset.Status);

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
