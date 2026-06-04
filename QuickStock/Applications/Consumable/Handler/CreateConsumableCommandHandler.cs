using QuickStock.CQRS;
using QuickStock.Infrastructure.Data;
using QuickStock.Domain.Consumable;
using QuickStock.Domain.Shared; 
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Http; 
using System.Security.Claims; 
using QuickStock.Common.Exceptions;
using QuickStock.Applications.Consumables.Commands;
using QuickStock.Applications.Consumables.Dto_s;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace QuickStock.Applications.Consumables.Handlers
{
    public class CreateConsumableCommandHandler : IRequestHandler<CreateConsumableCommand, ConsumableCreateResponse>
    {
        private readonly AppDbContext _db;
        private readonly IHttpContextAccessor _httpContextAccessor; 

        public CreateConsumableCommandHandler(AppDbContext db, IHttpContextAccessor httpContextAccessor)
        {
            _db = db;
            _httpContextAccessor = httpContextAccessor;
        }

        public async Task<ConsumableCreateResponse> Handle(CreateConsumableCommand request, CancellationToken cancellationToken)
        {
            if (string.IsNullOrWhiteSpace(request.ProductName))
            {
                throw new BadRequestException("Product name cannot be empty.");
            }

            if (!request.Count.HasValue || request.Count.Value < 0)
            {
                throw new BadRequestException("Quantity cannot be negative.");
            }

            var currentUserId = _httpContextAccessor.HttpContext?.User?.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "Unknown ID";
            var currentUsername = _httpContextAccessor.HttpContext?.User?.Identity?.Name ?? "System/Anonymous";
            
            // 📝 Everyone now creates a 'Pending' request to ensure tracking transparency
            var consumableRequest = new ConsumableRequest
            {
                RequestType = "Create",
                ProductName = request.ProductName,
                ProductType = request.ProductType ?? string.Empty,
                Count = request.Count.Value,
                Status = "Pending", 
                Timestamp = DateTime.UtcNow,
                RequestorId = currentUserId,
                RequestorName = currentUsername,
                CampusId = request.CampusId
            };

            await _db.ConsumableRequests.AddAsync(consumableRequest, cancellationToken);
            await _db.SaveChangesAsync(cancellationToken);

            return new ConsumableCreateResponse
            {
                Id = consumableRequest.Id,
                Message = "Inventory creation request submitted. Waiting for administrative review/approval."
            }; 
        }
    }
}