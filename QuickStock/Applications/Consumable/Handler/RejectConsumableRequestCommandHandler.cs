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
using Microsoft.EntityFrameworkCore;

namespace QuickStock.Applications.Consumables.Handlers
{
    public class RejectConsumableRequestCommandHandler : IRequestHandler<RejectConsumableRequestCommand, ConsumableCreateResponse>
    {
        private readonly AppDbContext _db;

        public RejectConsumableRequestCommandHandler(AppDbContext db)
        {
            _db = db;
        }

        public async Task<ConsumableCreateResponse> Handle(RejectConsumableRequestCommand request, CancellationToken cancellationToken)
        {
            var req = await _db.ConsumableRequests
                .FirstOrDefaultAsync(r => r.Id == request.Id, cancellationToken);

            if (req == null)
            {
                throw new NotFoundException($"Consumable Request with ID {request.Id} was not found.");
            }

            if (req.Status != "Pending")
            {
                throw new BadRequestException($"Request is already in '{req.Status}' state.");
            }

            if (string.IsNullOrWhiteSpace(request.RejectionReason))
            {
                throw new BadRequestException("A reason for rejection must be provided.");
            }

            var reviewerId = request.User?.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            var reviewerName = request.User?.Identity?.Name;

            req.Status = "Rejected";
            req.RejectionReason = request.RejectionReason;
            req.ReviewerId = reviewerId;
            req.ReviewerName = reviewerName;

            await _db.SaveChangesAsync(cancellationToken);

            return new ConsumableCreateResponse
            {
                Id = req.Id,
                Message = "Request rejected successfully."
            };
        }
    }
}
