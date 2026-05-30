using QuickStock.CQRS;
using QuickStock.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using QuickStock.Common.Exceptions;
using QuickStock.Applications.Consumables.Commands;
using QuickStock.Applications.Consumables.Dto_s;
using QuickStock.Domain.Shared;
using Microsoft.AspNetCore.Http;
using System.Security.Claims;

namespace QuickStock.Applications.Consumables.Handlers
{
    public class DeductStockCommandHandler : IRequestHandler<DeductStockCommand, ConsumableCreateResponse>
    {
        private readonly AppDbContext _db;
        private readonly IHttpContextAccessor _httpContextAccessor;

        public DeductStockCommandHandler(AppDbContext db, IHttpContextAccessor httpContextAccessor)
        {
            _db = db;
            _httpContextAccessor = httpContextAccessor;
        }

        public async Task<ConsumableCreateResponse> Handle(DeductStockCommand request, CancellationToken cancellationToken)
        {
            // 1. Guard Rail: Do not accept 0 or negative deduction numbers
            if (request.Quantity <= 0)
            {
                throw new BadRequestException("Please input quantity.");
            }

            // 2. Locate the existing item in the database
            var consumable = await _db.ConsumableUnits
                .FirstOrDefaultAsync(c => c.Id == request.Id, cancellationToken);

            if (consumable == null)
            {
                throw new NotFoundException($"Consumable item with ID {request.Id} was not found.");
            }

            // 3. Safety Check: Ensure the warehouse has enough stock to fulfill the deduction
            int currentCount = consumable.Count ?? 0;
            if (currentCount < request.Quantity)
            {
                throw new BadRequestException($"Insufficient stock. Current available stock is only {currentCount}.");
            }

            // 4. Subtract the math value safely
            consumable.Count = currentCount - request.Quantity;

            // 5. Save updates to MySQL
            await _db.SaveChangesAsync(cancellationToken);

            // 6. Create Audit Log
            var currentUserId = _httpContextAccessor.HttpContext?.User?.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            var currentUsername = _httpContextAccessor.HttpContext?.User?.Identity?.Name;

            var auditLog = new AuditLog
            {
                Action = "Deduct Stock",
                EntityType = "Consumable",
                EntityId = consumable.Id,
                EntityName = consumable.ProductName,
                Details = $"Type: {consumable.ProductType} | Count: {request.Quantity}",
                Timestamp = DateTime.UtcNow,
                UserId = currentUserId,
                Username = currentUsername,
                CampusId = consumable.CampusId,
                Status = "Deduct Quantity"
            };

            await _db.AuditLogs.AddAsync(auditLog, cancellationToken);
            await _db.SaveChangesAsync(cancellationToken);

            // 7. Return standard structured success message
            return new ConsumableCreateResponse
            {
                Id = consumable.Id,
                Message = "Deducted successfully" // Custom message for clarity
            };
        }
    }
}