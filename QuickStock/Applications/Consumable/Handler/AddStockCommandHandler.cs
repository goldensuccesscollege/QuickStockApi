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
            if (request.Quantity <= 0)
            {
                throw new BadRequestException("Please input quantity.");
            }

            var consumable = await _db.ConsumableUnits
                .FirstOrDefaultAsync(c => c.Id == request.Id, cancellationToken);

            if (consumable == null)
            {
                throw new NotFoundException($"Consumable item with ID {request.Id} was not found.");
            }

            var currentUserId = _httpContextAccessor.HttpContext?.User?.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "Unknown ID";
            var currentUsername = _httpContextAccessor.HttpContext?.User?.Identity?.Name ?? "System/Anonymous";

            // 📝 Route directly through the Requests table with a Pending status
            var consumableRequest = new ConsumableRequest
            {
                RequestType = "Add",
                ProductName = consumable.ProductName ?? "Unknown Product",
                ProductType = consumable.ProductType ?? "Unknown",
                Count = request.Quantity,
                TargetItemId = consumable.Id,
                Status = "Pending",
                Timestamp = DateTime.UtcNow,
                RequestorId = currentUserId,
                RequestorName = currentUsername,
                CampusId = consumable.CampusId
            };

            await _db.ConsumableRequests.AddAsync(consumableRequest, cancellationToken);
            await _db.SaveChangesAsync(cancellationToken);

            return new ConsumableCreateResponse
            {
                Id = consumableRequest.Id,
                Message = "Stock addition request submitted. Waiting for administrative review/approval."
            };
        }
    }
}