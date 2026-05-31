using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using QuickStock.CQRS;
using QuickStock.Infrastructure.Data;
using QuickStock.Applications.Consumables.Queries;
using QuickStock.Applications.Consumables.Dto_s;

namespace QuickStock.Applications.Consumable.Handler
{
    public class GetConsumableLedgerQueryHandler : IRequestHandler<GetConsumableLedgerQuery, List<ConsumableLedgerEntryDto>>
    {
        private readonly AppDbContext _context;

        public GetConsumableLedgerQueryHandler(AppDbContext context)
        {
            _context = context;
        }

        public async Task<List<ConsumableLedgerEntryDto>> Handle(GetConsumableLedgerQuery request, CancellationToken cancellationToken)
        {
            // 1. Pull all Consumable audit logs for the campus, ordered by oldest first
            //    so we can calculate the running balance correctly
            var query = _context.AuditLogs
                .AsNoTracking()
                .Where(l => l.EntityType == "Consumable");

            if (request.CampusId.HasValue)
            {
                query = query.Where(l => l.CampusId == request.CampusId.Value);
            }

            if (request.ProductId.HasValue)
            {
                query = query.Where(l => l.EntityId == request.ProductId.Value);
            }

            var logs = await query
                .OrderBy(l => l.EntityId)
                .ThenBy(l => l.Timestamp)
                .ToListAsync(cancellationToken);

            // 2. Group by product (EntityId), compute running balance per product
            var ledgerEntries = new List<ConsumableLedgerEntryDto>();

            var groupedByProduct = logs.GroupBy(l => l.EntityId);

            foreach (var productGroup in groupedByProduct)
            {
                int runningBalance = 0;

                foreach (var log in productGroup.OrderBy(l => l.Timestamp))
                {
                    // Parse the quantity from the Details field: "Type: pieces | Count: 10"
                    int quantity = ParseCount(log.Details);

                    int inQty = 0;
                    int outQty = 0;

                    if (log.Action == "Create" || log.Action == "Add Stock")
                    {
                        inQty = quantity;
                        runningBalance += quantity;
                    }
                    else if (log.Action == "Deduct Stock")
                    {
                        outQty = quantity;
                        runningBalance -= quantity;
                        if (runningBalance < 0) runningBalance = 0; // Safety guard
                    }
                    else
                    {
                        // Skip unrecognised action types (e.g. Approve/Reject)
                        continue;
                    }

                    ledgerEntries.Add(new ConsumableLedgerEntryDto
                    {
                        Date = log.Timestamp,
                        ProductId = log.EntityId,
                        ProductName = log.EntityName ?? string.Empty,
                        In = inQty,
                        Out = outQty,
                        Balance = runningBalance,
                        ProcessedByName = log.Username ?? "System"
                    });
                }
            }

            // 3. Return all ledger entries sorted by most recent first
            return ledgerEntries
                .OrderByDescending(e => e.Date)
                .ToList();
        }

        /// <summary>
        /// Parses the Count value from the AuditLog Details string.
        /// Format example: "Type: pieces | Count: 10"
        /// </summary>
        private static int ParseCount(string? details)
        {
            if (string.IsNullOrEmpty(details)) return 0;

            var parts = details.Split('|');
            foreach (var part in parts)
            {
                var trimmed = part.Trim();
                if (trimmed.StartsWith("Count:", StringComparison.OrdinalIgnoreCase))
                {
                    var valueStr = trimmed.Substring("Count:".Length).Trim();
                    if (int.TryParse(valueStr, out int count))
                        return count;
                }
            }
            return 0;
        }
    }
}
