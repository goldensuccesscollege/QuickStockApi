using QuickStock.CQRS;
using QuickStock.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using QuickStock.Common.Exceptions;
using QuickStock.Applications.Consumables.Commands;
using QuickStock.Applications.Consumables.Dto_s;
using QuickStock.Domain.Shared;
using QuickStock.Domain.Consumable;
using Microsoft.AspNetCore.Http;
using System.Security.Claims;
using System.Threading;
using System.Threading.Tasks;
using System;

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
            if (request.Quantity <= 0)
            {
                throw new BadRequestException("Please input a valid quantity greater than zero.");
            }

            var consumable = await _db.ConsumableUnits
                .FirstOrDefaultAsync(c => c.Id == request.Id, cancellationToken);

            if (consumable == null)
            {
                throw new NotFoundException($"Consumable item with ID {request.Id} was not found.");
            }

            int currentCount = consumable.Count ?? 0;
            if (currentCount < request.Quantity)
            {
                throw new BadRequestException($"Insufficient stock available. Current stock: {currentCount}.");
            }

            var currentUserId = _httpContextAccessor.HttpContext?.User?.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "Unknown ID";
            var currentUsername = _httpContextAccessor.HttpContext?.User?.Identity?.Name ?? "System/Anonymous";

            // 📝 Safeguard database schema constraints with strict fallback replacements
            var consumableRequest = new ConsumableRequest
            {
                RequestType = "Deduct",
                ProductName = !string.IsNullOrWhiteSpace(consumable.ProductName) ? consumable.ProductName : "Unknown Item",
                ProductType = !string.IsNullOrWhiteSpace(consumable.ProductType) ? consumable.ProductType : "Unit",
                Count = request.Quantity,
                TargetItemId = consumable.Id,
                Status = "Pending", 
                Timestamp = DateTime.UtcNow,
                RequestorId = currentUserId,
                RequestorName = currentUsername,
                CampusId = consumable.CampusId != 0 ? consumable.CampusId : 1 // Fallback to primary campus context if zero
            };

            try 
            {
                await _db.ConsumableRequests.AddAsync(consumableRequest, cancellationToken);
                await _db.SaveChangesAsync(cancellationToken);
            }
            catch (Exception ex)
            {
                // Surface inner database exceptions clearly to help troubleshoot missing table configurations
                throw new BadRequestException($"Database save failed. Ensure schema constraints match. Detail: {ex.InnerException?.Message ?? ex.Message}");
            }

            return new ConsumableCreateResponse
            {
                Id = consumableRequest.Id,
                Message = "Stock deduction request submitted successfully. Waiting for review."
            };
        }
    }
}