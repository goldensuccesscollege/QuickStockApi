using QuickStock.CQRS;
using Microsoft.EntityFrameworkCore;
using QuickStock.Applications.Itassets.Command;
using QuickStock.Infrastructure.Data;
using QuickStock.Domain.ITassets;
using QuickStock.Domain.Locations;
using QuickStock.Domain.Shared;
using QuickStock.Domain.Accounts;
using System;
using System.Collections.Generic;
using System.Security.Claims;
using System.Threading;
using System.Threading.Tasks;

namespace QuickStock.Applications.Itassets.Handler
{
    public class TransferItAssetHandler : IRequestHandler<TransferItAssetCommand, object>
    {
        private readonly AppDbContext _context;

        public TransferItAssetHandler(AppDbContext context)
        {
            _context = context;
        }

        public async Task<object> Handle(TransferItAssetCommand request, CancellationToken cancellationToken)
        {
            var asset = await _context.Itassets
                .Include(a => a.Room)
                .FirstOrDefaultAsync(a => a.Id == request.Id, cancellationToken);

            if (asset == null)
                throw new KeyNotFoundException("Asset not found.");

            var oldRoomName = asset.Room?.RoomName ?? "Unknown";
            var targetRoom = await _context.Rooms.FindAsync(new object[] { request.TargetRoomId }, cancellationToken);

            if (targetRoom == null)
                throw new InvalidOperationException("Target room not found.");

            asset.RoomId = request.TargetRoomId;
            asset.Location = targetRoom.RoomName;
            await _context.SaveChangesAsync(cancellationToken);

            await LogAction(request.User, "Transfer", asset.Id, asset.Name,
                $"Transferred from {oldRoomName} to {targetRoom.RoomName}", asset.CampusId, asset.Status);

            return new { success = true, roomName = targetRoom.RoomName };
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
