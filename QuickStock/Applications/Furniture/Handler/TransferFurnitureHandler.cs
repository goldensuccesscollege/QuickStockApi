using MediatR;
using Microsoft.EntityFrameworkCore;
using QuickStock.Applications.Furniture.Command;
using QuickStock.Infrastructure.Data;
using QuickStock.Domain.Shared;
using System.Security.Claims;
using System.Threading;
using System.Threading.Tasks;

namespace QuickStock.Applications.Furniture.Handler
{
    public class TransferFurnitureHandler : IRequestHandler<TransferFurnitureCommand, object>
    {
        private readonly AppDbContext _context;

        public TransferFurnitureHandler(AppDbContext context)
        {
            _context = context;
        }

        public async Task<object> Handle(TransferFurnitureCommand request, CancellationToken cancellationToken)
        {
            var furniture = await _context.Furnitures
                .Include(f => f.Room)
                .FirstOrDefaultAsync(f => f.Item_ID == request.Id, cancellationToken);

            if (furniture == null) throw new KeyNotFoundException("Furniture not found.");

            var oldLocation = furniture.Location ?? "Unknown";
            var targetRoom = await _context.Rooms.FindAsync(new object[] { request.TargetRoomId }, cancellationToken);

            if (targetRoom == null) throw new InvalidOperationException("Target room not found.");

            furniture.RoomId = request.TargetRoomId;
            furniture.Location = targetRoom.RoomName;

            await _context.SaveChangesAsync(cancellationToken);

            await LogAction(request.User, "Transfer", furniture.Item_ID, furniture.Item_Name,
                $"Transferred from {oldLocation} to {targetRoom.RoomName}", furniture.CampusId, furniture.Condition);

            return new { success = true, location = targetRoom.RoomName };
        }

        private async Task LogAction(ClaimsPrincipal user, string action, int entityId, string entityName, string details, int campusId, string? status)
        {
            var log = new AuditLog
            {
                Action = action,
                EntityType = "Furniture",
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
