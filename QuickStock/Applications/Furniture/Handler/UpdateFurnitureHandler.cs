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
    public class UpdateFurnitureHandler : IRequestHandler<UpdateFurnitureCommand, Domain.Furniture.Furniture>
    {
        private readonly AppDbContext _context;

        public UpdateFurnitureHandler(AppDbContext context)
        {
            _context = context;
        }

        public async Task<Domain.Furniture.Furniture> Handle(UpdateFurnitureCommand request, CancellationToken cancellationToken)
        {
            var existing = await _context.Furnitures.FindAsync(new object[] { request.Furniture.Item_ID }, cancellationToken);
            if (existing == null) throw new KeyNotFoundException("Furniture not found.");

            // Update fields
            existing.Item_Name = request.Furniture.Item_Name;
            existing.Item_number = request.Furniture.Item_number;
            existing.Brand = request.Furniture.Brand;
            existing.Condition = request.Furniture.Condition;
            existing.Item_count = request.Furniture.Item_count;
            // Room/Location might be updated via transfer, but we allow it here too if provided
            if (request.Furniture.RoomId.HasValue && request.Furniture.RoomId != existing.RoomId)
            {
                var room = await _context.Rooms.FindAsync(new object[] { request.Furniture.RoomId.Value }, cancellationToken);
                if (room != null)
                {
                    existing.RoomId = request.Furniture.RoomId;
                    existing.Location = room.RoomName;
                }
            }

            await _context.SaveChangesAsync(cancellationToken);

            await LogAction(request.User, "Update", existing.Item_ID, existing.Item_Name,
                $"Updated furniture details: {existing.Item_Name}", existing.CampusId, existing.Condition);

            return existing;
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
