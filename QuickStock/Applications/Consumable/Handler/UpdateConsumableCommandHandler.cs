using QuickStock.CQRS;
using QuickStock.Infrastructure.Data;
using QuickStock.Domain.Shared;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Http;
using System.Security.Claims;
using QuickStock.Common.Exceptions;
using QuickStock.Applications.Consumables.Commands;
using QuickStock.Applications.Consumables.Dto_s;

namespace QuickStock.Applications.Consumables.Handlers
{
    public class UpdateConsumableCommandHandler : IRequestHandler<UpdateConsumableCommand, ConsumableCreateResponse>
    {
        private readonly AppDbContext _db;
        private readonly IHttpContextAccessor _httpContextAccessor;

        public UpdateConsumableCommandHandler(AppDbContext db, IHttpContextAccessor httpContextAccessor)
        {
            _db = db;
            _httpContextAccessor = httpContextAccessor;
        }

        public async Task<ConsumableCreateResponse> Handle(UpdateConsumableCommand request, CancellationToken cancellationToken)
        {
            // 1. Find the existing consumable
            var consumable = await _db.ConsumableUnits
                .FirstOrDefaultAsync(c => c.Id == request.Id, cancellationToken);

            if (consumable == null)
                throw new NotFoundException($"Consumable with ID {request.Id} was not found.");

            // 2. Validate inputs
            if (string.IsNullOrWhiteSpace(request.ProductName))
                throw new BadRequestException("Product name cannot be empty.");

            if (request.Count.HasValue && request.Count.Value < 0)
                throw new BadRequestException("Quantity cannot be negative.");

            // 3. Apply updates
            var oldCount = consumable.Count;
            consumable.ProductName = request.ProductName ?? consumable.ProductName;
            consumable.ProductType  = request.ProductType  ?? consumable.ProductType;
            consumable.Count        = request.Count.HasValue ? request.Count.Value : consumable.Count;
            consumable.DateArrive   = request.DateArrive   ?? consumable.DateArrive;

            await _db.SaveChangesAsync(cancellationToken);

            // 4. Write audit log
            // 🔒 FIX: Added ?? fallbacks here to completely eliminate warning CS8601
            var currentUserId   = _httpContextAccessor.HttpContext?.User?.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "Unknown ID";
            var currentUsername = _httpContextAccessor.HttpContext?.User?.Identity?.Name ?? "System/Anonymous";

            var auditLog = new AuditLog
            {
                Action     = "Update",
                EntityType = "Consumable",
                EntityId   = consumable.Id,
                EntityName = consumable.ProductName ?? "Unknown Product", // Added absolute safety fallback
                Details    = $"Type: {consumable.ProductType ?? "Unknown"} | Count: {consumable.Count}",
                Timestamp  = DateTime.UtcNow,
                UserId     = currentUserId,
                Username   = currentUsername,
                CampusId   = consumable.CampusId,
                Status     = "Update Unit"
            };

            await _db.AuditLogs.AddAsync(auditLog, cancellationToken);
            await _db.SaveChangesAsync(cancellationToken);

            return new ConsumableCreateResponse
            {
                Id      = consumable.Id,
                Message = "Consumable updated successfully."
            };
        }
    }
}