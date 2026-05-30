using QuickStock.CQRS;
using QuickStock.Infrastructure.Data;
using QuickStock.Domain.Consumable;
using QuickStock.Domain.Shared; // 💡 Ensure this is imported for your AuditLog entity
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Http; // 💡 Required to read the logged-in user's HTTP context
using System.Security.Claims; // 💡 Required to find the user's NameIdentifier claim
using QuickStock.Common.Exceptions;
using QuickStock.Applications.Consumables.Commands;
using QuickStock.Applications.Consumables.Dto_s;

namespace QuickStock.Applications.Consumables.Handlers
{
    public class CreateConsumableCommandHandler : IRequestHandler<CreateConsumableCommand, ConsumableCreateResponse>
    {
        private readonly AppDbContext _db;
        private readonly IHttpContextAccessor _httpContextAccessor; // 💡 Injected property

        // Constructor updated to receive the HttpContextAccessor alongside the DbContext
        public CreateConsumableCommandHandler(AppDbContext db, IHttpContextAccessor httpContextAccessor)
        {
            _db = db;
            _httpContextAccessor = httpContextAccessor;
        }

        public async Task<ConsumableCreateResponse> Handle(CreateConsumableCommand request, CancellationToken cancellationToken)
        {
            // 1. Validation Guard Rails
            if (string.IsNullOrWhiteSpace(request.ProductName))
            {
                throw new BadRequestException("Product name cannot be empty.");
            }

            // Blocks 0 and negative values, then throws your custom message
            if (!request.Count.HasValue || request.Count.Value <= 0)
            {
                throw new BadRequestException("Please input quantity.");
            }

            // 2. Validate Foreign Key Existence 
            var campusExists = await _db.Campuses
                .AnyAsync(c => c.CampusId == request.CampusId, cancellationToken);

            if (!campusExists)
            {
                throw new NotFoundException($"Target Campus ID {request.CampusId} does not exist.");
            }

            // 3. Construct Domain Model Map
            var consumable = new ConsumableUnit
            {
                ProductName = request.ProductName,
                ProductType = request.ProductType,
                Count = request.Count.Value, 
                DateArrive = request.DateArrive ?? DateTime.UtcNow,
                CampusId = request.CampusId
            };

            // 4. Save to Database
            await _db.ConsumableUnits.AddAsync(consumable, cancellationToken);
            await _db.SaveChangesAsync(cancellationToken);

            // ==========================================
            // 📝 NEW: CREATE AUDIT LOG ENTRY
            // ==========================================
            
            // Extract the authenticated user info from the JWT token claims context
            var currentUserId = _httpContextAccessor.HttpContext?.User?.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            var currentUsername = _httpContextAccessor.HttpContext?.User?.Identity?.Name;

            var auditLog = new AuditLog
            {
                Action = "Create",
                EntityType = "Consumable",
                EntityId = consumable.Id,
                EntityName = consumable.ProductName,
                Details = $"Type: {consumable.ProductType} | Count: {consumable.Count}",
                Timestamp = DateTime.UtcNow,
                UserId = currentUserId,
                Username = currentUsername, // Used fallback if name mapping hasn't calculated yet
                CampusId = consumable.CampusId,
                Status = "Create Unit" // Populates Status
            };

            await _db.AuditLogs.AddAsync(auditLog, cancellationToken);
            await _db.SaveChangesAsync(cancellationToken);

            // ==========================================

            // 5. Return success response
            return new ConsumableCreateResponse
            {
                Id = consumable.Id,
                Message = "Added successfully"
            }; 
        }
    }
}