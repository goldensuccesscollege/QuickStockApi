using QuickStock.CQRS;
using QuickStock.Infrastructure.Data;
using QuickStock.Domain.Consumable;
using System;
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
            // --- EXISTING VALIDATIONS ---
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

            // 👇 CHANGED THIS: Grab the RequestorId directly from the command string
            var currentUserId = request.RequestorId;

            // --- MAP TO DOMAIN ENTITY ---
            var consumableRequest = new ConsumableRequest
            {
                RequestType = request.RequestType,
                ProductName = request.ProductName,
                ProductType = request.ProductType,
                Count = request.Count,
                TargetItemId = request.TargetItemId,
                Status = "Pending",
                Timestamp = request.Timestamp ?? DateTime.UtcNow,
                RequestorId = currentUserId, // Saved safely as a string
                RequestorName = request.RequestorName, // Map requestor name
                CampusId = request.CampusId
            };

            // --- SAVE TO DATABASE ---
            await _db.ConsumableRequests.AddAsync(consumableRequest, cancellationToken);
            await _db.SaveChangesAsync(cancellationToken);

            // 👇 2. GENERATE A NEW TOKEN FOR THE NEXT TRANSACTION
            string generatedNextToken = Guid.NewGuid().ToString("N");

            // --- RETURN THE RESPONSE ---
            return new ConsumableCreateResponse
            {
                Id = consumableRequest.Id,
                Message = "Request submitted successfully. Waiting for review.",
                
                // SEND THE FRESH TOKEN TO THE FRONTEND
                NextSubmitToken = generatedNextToken 
            };
        }
    }
}