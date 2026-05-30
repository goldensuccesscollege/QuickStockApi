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
    public class AddStockCommandHandler : IRequestHandler<AddStockCommand, ConsumableCreateResponse>
    {
        private readonly AppDbContext _db;
        private readonly IHttpContextAccessor _httpContextAccessor;

        public AddStockCommandHandler(AppDbContext db, IHttpContextAccessor httpContextAccessor)
        {
            _db = db;
            _httpContextAccessor = httpContextAccessor;
        }

        public async Task<ConsumableCreateResponse> Handle(AddStockCommand request, CancellationToken cancellationToken)
        {
            // 1. Guard Rail: Do not accept 0 or negative top-up adjustments
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

            // 3. Increment the math value safely
            // (Using ?? 0 just in case the existing DB field value was somehow NULL)
            consumable.Count = (consumable.Count ?? 0) + request.Quantity;
            consumable.DateArrive = DateTime.UtcNow; // Update arrival date to reflect latest delivery

            // 4. Save updates to MySQL
            await _db.SaveChangesAsync(cancellationToken);

            // 5. Create Audit Log
            var currentUserId = _httpContextAccessor.HttpContext?.User?.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            var currentUsername = _httpContextAccessor.HttpContext?.User?.Identity?.Name;

            var auditLog = new AuditLog
            {
                Action = "Add Stock",
                EntityType = "Consumable",
                EntityId = consumable.Id,
                EntityName = consumable.ProductName,
                Details = $"Type: {consumable.ProductType} | Count: {request.Quantity}",
                Timestamp = DateTime.UtcNow,
                UserId = currentUserId,
                Username = currentUsername,
                CampusId = consumable.CampusId,
                Status = "Add Quantity"
            };

            await _db.AuditLogs.AddAsync(auditLog, cancellationToken);
            await _db.SaveChangesAsync(cancellationToken);

            // 6. Return clean structured feedback loop payload
            return new ConsumableCreateResponse
            {
                Id = consumable.Id,
                Message = "Added successfully"
            };
        }
    }
}