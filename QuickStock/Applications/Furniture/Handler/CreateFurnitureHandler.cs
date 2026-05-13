using MediatR;
using Microsoft.EntityFrameworkCore;
using QuickStock.Applications.Furniture.Command;
using QuickStock.Infrastructure.Data;
using QuickStock.Domain.Furniture;
using QuickStock.Domain.Shared;
using QuickStock.Infrastructure.Services;
using System.Security.Claims;

namespace QuickStock.Applications.Furniture.Handler
{
    public class CreateFurnitureHandler : IRequestHandler<CreateFurnitureCommand, Domain.Furniture.Furniture>
    {
        private readonly AppDbContext _context;
        private readonly INotificationService _notificationService;

        public CreateFurnitureHandler(AppDbContext context, INotificationService notificationService)
        {
            _context = context;
            _notificationService = notificationService;
        }

        public async Task<Domain.Furniture.Furniture> Handle(CreateFurnitureCommand request, CancellationToken cancellationToken)
        {
            var furniture = request.Furniture;

            // Check for duplicate Item_number or Item_ID within the same campus
            if (!string.IsNullOrWhiteSpace(furniture.Item_number))
            {
                var duplicate = await _context.Furnitures.AnyAsync(
                    f => f.Item_number == furniture.Item_number && f.CampusId == furniture.CampusId,
                    cancellationToken);
                if (duplicate)
                    throw new InvalidOperationException("Furniture with this Item Number already exists in this campus.");
            }

            // Generate QR Code if not provided
            if (string.IsNullOrWhiteSpace(furniture.Qrcode))
            {
                furniture.Qrcode = "FUR-" + Guid.NewGuid().ToString("N").Substring(0, 8).ToUpper();
            }

            furniture.DateAdded = DateTime.UtcNow;
            
            _context.Furnitures.Add(furniture);
            await _context.SaveChangesAsync(cancellationToken);

            // Log Action
            await LogAction(request.User, "Add", furniture.Item_ID, furniture.Item_Name,
                $"Added new furniture: {furniture.Item_Name} (QR: {furniture.Qrcode}, Count: {furniture.Item_count})", 
                furniture.CampusId, furniture.Condition);


            // Send Real-time Notification
            await _notificationService.NotifyCampusActivity(furniture.CampusId, "New Furniture Added", 
                $"{request.User.Identity?.Name} added {furniture.Item_Name} to {furniture.Location}", "Success");

            return furniture;
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
