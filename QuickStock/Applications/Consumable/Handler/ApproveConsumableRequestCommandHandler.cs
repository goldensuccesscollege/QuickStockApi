using QuickStock.CQRS;
using QuickStock.Infrastructure.Data;
using QuickStock.Domain.Consumable;
using QuickStock.Domain.Shared;
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
    public class ApproveConsumableRequestCommandHandler : IRequestHandler<ApproveConsumableRequestCommand, ConsumableCreateResponse>
    {
        private readonly AppDbContext _db;

        public ApproveConsumableRequestCommandHandler(AppDbContext db)
        {
            _db = db;
        }

        public async Task<ConsumableCreateResponse> Handle(ApproveConsumableRequestCommand request, CancellationToken cancellationToken)
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

            var reviewerId = request.User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            var reviewerName = request.User.Identity?.Name;

            int finalEntityId = 0;
            string auditAction = string.Empty;
            string auditStatus = string.Empty;
            string auditDetails = string.Empty;

            if (req.RequestType == "Create")
            {
                var consumable = new ConsumableUnit
                {
                    ProductName = req.ProductName,
                    ProductType = req.ProductType,
                    Count = req.Count,
                    DateArrive = DateTime.UtcNow,
                    CampusId = req.CampusId
                };

                await _db.ConsumableUnits.AddAsync(consumable, cancellationToken);
                await _db.SaveChangesAsync(cancellationToken);

                req.TargetItemId = consumable.Id;
                finalEntityId = consumable.Id;
                auditAction = "Create";
                auditStatus = "Create Unit";
                auditDetails = $"Type: {consumable.ProductType} | Count: {consumable.Count}";
            }
            else if (req.RequestType == "Add")
            {
                var consumable = await _db.ConsumableUnits
                    .FirstOrDefaultAsync(c => c.Id == req.TargetItemId, cancellationToken);

                if (consumable == null)
                {
                    throw new NotFoundException($"Consumable item with ID {req.TargetItemId} was not found.");
                }

                consumable.Count = (consumable.Count ?? 0) + req.Count;
                consumable.DateArrive = DateTime.UtcNow;
                finalEntityId = consumable.Id;
                auditAction = "Add Stock";
                auditStatus = "Add Quantity";
                auditDetails = $"Type: {consumable.ProductType} | Count: {req.Count}";
            }
            else if (req.RequestType == "Deduct")
            {
                var consumable = await _db.ConsumableUnits
                    .FirstOrDefaultAsync(c => c.Id == req.TargetItemId, cancellationToken);

                if (consumable == null)
                {
                    throw new NotFoundException($"Consumable item with ID {req.TargetItemId} was not found.");
                }

                int currentCount = consumable.Count ?? 0;
                if (currentCount < req.Count)
                {
                    throw new BadRequestException($"Insufficient stock. Current available stock is only {currentCount}.");
                }

                consumable.Count = currentCount - req.Count;
                finalEntityId = consumable.Id;
                auditAction = "Deduct Stock";
                auditStatus = "Deduct Quantity";
                auditDetails = $"Type: {consumable.ProductType} | Count: {req.Count}";
            }
            else
            {
                throw new BadRequestException($"Unsupported request type '{req.RequestType}'.");
            }

            req.Status = "Approved";
            req.ReviewerId = reviewerId;
            req.ReviewerName = reviewerName;

            await _db.SaveChangesAsync(cancellationToken);

            // Generate Audit Log representing the actual completed action
            var auditLog = new AuditLog
            {
                Action = auditAction,
                EntityType = "Consumable",
                EntityId = finalEntityId,
                EntityName = req.ProductName,
                Details = auditDetails,
                Timestamp = DateTime.UtcNow,
                UserId = req.RequestorId,
                Username = req.RequestorName, // Requestor gets credit for the action
                CampusId = req.CampusId,
                Status = auditStatus
            };

            await _db.AuditLogs.AddAsync(auditLog, cancellationToken);
            await _db.SaveChangesAsync(cancellationToken);

            return new ConsumableCreateResponse
            {
                Id = req.Id,
                Message = "Request approved successfully and applied to inventory."
            };
        }
    }
}
