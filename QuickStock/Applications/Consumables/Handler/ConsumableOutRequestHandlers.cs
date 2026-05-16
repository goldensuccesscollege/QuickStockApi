using MediatR;
using Microsoft.EntityFrameworkCore;
using QuickStock.Applications.Consumables.Command;
using QuickStock.Applications.Consumables.Queries;
using QuickStock.Domain.Consumables;
using QuickStock.Domain.Shared;
using QuickStock.Infrastructure.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading;
using System.Threading.Tasks;

namespace QuickStock.Applications.Consumables.Handler
{
    // ---------- Get Pending Requests ----------
    public class GetPendingOutRequestsHandler : IRequestHandler<GetPendingOutRequestsQuery, List<ConsumableOutRequest>>
    {
        private readonly AppDbContext _context;
        public GetPendingOutRequestsHandler(AppDbContext context) { _context = context; }

        public async Task<List<ConsumableOutRequest>> Handle(GetPendingOutRequestsQuery request, CancellationToken cancellationToken)
        {
            return await _context.ConsumableOutRequests
                .Where(r => r.CampusId == request.CampusId && r.Status == "Pending")
                .OrderByDescending(r => r.RequestedAt)
                .ToListAsync(cancellationToken);
        }
    }

    public class GetOutRequestHistoryHandler : IRequestHandler<GetOutRequestHistoryQuery, List<ConsumableOutRequest>>
    {
        private readonly AppDbContext _context;
        public GetOutRequestHistoryHandler(AppDbContext context) { _context = context; }

        public async Task<List<ConsumableOutRequest>> Handle(GetOutRequestHistoryQuery request, CancellationToken cancellationToken)
        {
            var query = _context.ConsumableOutRequests
                .Where(r => r.CampusId == request.CampusId);

            if (!string.IsNullOrEmpty(request.Status))
            {
                query = query.Where(r => r.Status == request.Status);
            }

            if (!string.IsNullOrEmpty(request.RequestedByUserId))
            {
                query = query.Where(r => r.RequestedByUserId == request.RequestedByUserId);
            }

            return await query
                .OrderByDescending(r => r.RequestedAt)
                .Take(50) // Limit to last 50 requests
                .ToListAsync(cancellationToken);
        }
    }

    // ---------- Request Item Out ----------
    public class RequestItemOutHandler : IRequestHandler<RequestItemOutCommand, (bool Success, string Message)>
    {
        private readonly AppDbContext _context;
        public RequestItemOutHandler(AppDbContext context) { _context = context; }

        public async Task<(bool Success, string Message)> Handle(RequestItemOutCommand request, CancellationToken cancellationToken)
        {
            var item = await _context.ConsumableItems
                .Include(i => i.ConsumableData)
                .FirstOrDefaultAsync(i => i.Id == request.ItemId, cancellationToken);

            if (item == null) return (false, "Item not found.");
            if (item.Status != "In Stock") return (false, "Item is not currently In Stock.");

            // Check if there's already a pending request for this item
            var existing = await _context.ConsumableOutRequests
                .AnyAsync(r => r.ConsumableItemId == request.ItemId && r.Status == "Pending", cancellationToken);
            if (existing) return (false, "A pending request already exists for this item.");

            var userId = request.User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "";
            var username = request.User.Identity?.Name ?? "Unknown";

            var outRequest = new ConsumableOutRequest
            {
                ConsumableItemId = item.Id,
                ItemCode = item.ItemCode,
                ProductName = item.ConsumableData.Product,
                ConsumableDataId = item.ConsumableData.Id,
                RequestedByUserId = userId,
                RequestedByUsername = username,
                RequestedAt = DateTime.UtcNow,
                Status = "Pending",
                CampusId = item.ConsumableData.CampusId
            };

            _context.ConsumableOutRequests.Add(outRequest);
            await _context.SaveChangesAsync(cancellationToken);

            return (true, "Out request submitted. Awaiting approval.");
        }
    }

    // ---------- Approve Request ----------
    public class ApproveOutRequestHandler : IRequestHandler<ApproveOutRequestCommand, (bool Success, string Message)>
    {
        private readonly AppDbContext _context;
        public ApproveOutRequestHandler(AppDbContext context) { _context = context; }

        public async Task<(bool Success, string Message)> Handle(ApproveOutRequestCommand request, CancellationToken cancellationToken)
        {
            var outRequest = await _context.ConsumableOutRequests
                .FirstOrDefaultAsync(r => r.Id == request.RequestId && r.Status == "Pending", cancellationToken);

            if (outRequest == null) return (false, "Request not found or already processed.");

            var item = await _context.ConsumableItems
                .Include(i => i.ConsumableData)
                .FirstOrDefaultAsync(i => i.Id == outRequest.ConsumableItemId, cancellationToken);

            if (item == null) return (false, "Consumable item no longer exists.");

            var approverId = request.Approver.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "";
            var approverName = request.Approver.Identity?.Name ?? "Unknown";

            // Apply the actual status change
            item.Status = "Out";
            item.DateOut = DateTime.UtcNow;
            item.ConsumableData.Out++;

            // Update the request record
            outRequest.Status = "Approved";
            outRequest.ApprovedByUserId = approverId;
            outRequest.ApprovedByUsername = approverName;
            outRequest.ApprovedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync(cancellationToken);

            // Write a detailed audit log
            _context.AuditLogs.Add(new AuditLog
            {
                Action = "Inventory Out",
                EntityType = "Consumable",
                EntityId = item.ConsumableData.Id,
                EntityName = item.ConsumableData.Product,
                Details = $"Item '{item.ItemCode}' marked Out. " +
                          $"Requested by: {outRequest.RequestedByUsername} on {outRequest.RequestedAt:yyyy-MM-dd HH:mm}. " +
                          $"Approved by: {approverName} on {DateTime.UtcNow:yyyy-MM-dd HH:mm}. " +
                          $"New balance: {item.ConsumableData.In - item.ConsumableData.Out}.",
                UserId = approverId,
                Username = approverName,
                CampusId = item.ConsumableData.CampusId,
                Status = "Approved",
                Timestamp = DateTime.UtcNow
            });
            await _context.SaveChangesAsync(cancellationToken);

            return (true, "Request approved. Item is now Out.");
        }
    }

    // ---------- Reject Request ----------
    public class RejectOutRequestHandler : IRequestHandler<RejectOutRequestCommand, (bool Success, string Message)>
    {
        private readonly AppDbContext _context;
        public RejectOutRequestHandler(AppDbContext context) { _context = context; }

        public async Task<(bool Success, string Message)> Handle(RejectOutRequestCommand request, CancellationToken cancellationToken)
        {
            var outRequest = await _context.ConsumableOutRequests
                .FirstOrDefaultAsync(r => r.Id == request.RequestId && r.Status == "Pending", cancellationToken);

            if (outRequest == null) return (false, "Request not found or already processed.");

            var approverName = request.Approver.Identity?.Name ?? "Unknown";

            outRequest.Status = "Rejected";
            outRequest.ApprovedByUserId = request.Approver.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            outRequest.ApprovedByUsername = approverName;
            outRequest.ApprovedAt = DateTime.UtcNow;
            outRequest.Remarks = request.Reason;

            await _context.SaveChangesAsync(cancellationToken);

            return (true, "Request rejected.");
        }
    }

    // ---------- Cancel Request ----------
    public class CancelOutRequestHandler : IRequestHandler<CancelOutRequestCommand, (bool Success, string Message)>
    {
        private readonly AppDbContext _context;
        public CancelOutRequestHandler(AppDbContext context) { _context = context; }

        public async Task<(bool Success, string Message)> Handle(CancelOutRequestCommand request, CancellationToken cancellationToken)
        {
            var outRequest = await _context.ConsumableOutRequests
                .FirstOrDefaultAsync(r => r.Id == request.RequestId && r.Status == "Pending", cancellationToken);

            if (outRequest == null) return (false, "Request not found or already processed.");

            var userId = request.User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            var isAdmin = request.User.IsInRole("Admin") || request.User.IsInRole("Manager");

            if (outRequest.RequestedByUserId != userId && !isAdmin)
            {
                return (false, "You are not authorized to cancel this request.");
            }

            outRequest.Status = "Cancelled";
            await _context.SaveChangesAsync(cancellationToken);

            return (true, "Request cancelled successfully.");
        }
    }
}
