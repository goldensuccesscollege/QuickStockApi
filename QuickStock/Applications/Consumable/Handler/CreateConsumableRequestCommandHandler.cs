using QuickStock.CQRS;
using QuickStock.Infrastructure.Data;
using QuickStock.Domain.Consumable;
using System;
using System.Security.Claims;
using System.Threading;
using System.Threading.Tasks;
using QuickStock.Common.Exceptions;
using QuickStock.Applications.Consumables.Commands;
using QuickStock.Applications.Consumables.Dto_s;

namespace QuickStock.Applications.Consumables.Handlers
{
    public class CreateConsumableRequestCommandHandler : IRequestHandler<CreateConsumableRequestCommand, ConsumableCreateResponse>
    {
        private readonly AppDbContext _db;

        public CreateConsumableRequestCommandHandler(AppDbContext db)
        {
            _db = db;
        }

        public async Task<ConsumableCreateResponse> Handle(CreateConsumableRequestCommand request, CancellationToken cancellationToken)
        {
            if (string.IsNullOrWhiteSpace(request.ProductName))
            {
                throw new BadRequestException("Product Name is required.");
            }

            if (string.IsNullOrWhiteSpace(request.ProductType))
            {
                throw new BadRequestException("Product Type is required.");
            }

            if (request.Count <= 0)
            {
                throw new BadRequestException("Count must be greater than zero.");
            }

            var currentUserId = request.User?.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            var currentUsername = request.User?.Identity?.Name;

            var consumableRequest = new ConsumableRequest
            {
                RequestType = request.RequestType,
                ProductName = request.ProductName,
                ProductType = request.ProductType,
                Count = request.Count,
                TargetItemId = request.TargetItemId,
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
                Message = "Request submitted successfully. Waiting for review."
            };
        }
    }
}
