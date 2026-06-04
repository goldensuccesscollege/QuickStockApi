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
            // 1. Pull all Consumable audit logs matching your criteria
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

            // 🔒 FIX: Order strictly by Timestamp first to maintain absolute chronological reality across records
            var logs = await query
                .OrderBy(l => l.Timestamp)
                .ToListAsync(cancellationToken);

            var ledgerEntries = new List<ConsumableLedgerEntryDto>();

            // 2. Track independent product running balances via a dictionary lookup mapping
            var productBalances = new Dictionary<int, int>();

            foreach (var log in logs)
            {
                int quantity = ParseCount(log.Details);
                int inQty = 0;
                int outQty = 0;

                // Initialize balance map item entry if this is a newly discovered ID
                if (!productBalances.ContainsKey(log.EntityId))
                {
                    productBalances[log.EntityId] = 0;
                }

                if (log.Action == "Create" || log.Action == "Add Stock")
                {
                    inQty = quantity;
                    productBalances[log.EntityId] += quantity;
                }
                else if (log.Action == "Deduct Stock")
                {
                    outQty = quantity;
                    productBalances[log.EntityId] -= quantity;
                    
                    if (productBalances[log.EntityId] < 0) 
                    {
                        productBalances[log.EntityId] = 0; // Safety guard
                    }
                }
                else
                {
                    // Skip unrecognized structural logs safely
                    continue;
                }

                ledgerEntries.Add(new ConsumableLedgerEntryDto
                {
                    Date = log.Timestamp,
                    ProductId = log.EntityId,
                    ProductName = log.EntityName ?? string.Empty,
                    In = inQty,
                    Out = outQty,
                    Balance = productBalances[log.EntityId], // Tracks correct historical item context snapshot
                    ProcessedByName = log.Username ?? "System"
                });
            }

            // 3. Return all historical updates showing the newest transactions first
            return ledgerEntries
                .OrderByDescending(e => e.Date)
                .ToList();
        }

        /// <summary>
        /// Parses the Count value from the AuditLog Details string.
        /// Robust parsing structure handles accidental whitespace variations cleanly.
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
                    
                    // 🔒 FIX: Extracts purely numeric digits up until whitespace or metadata blocks to protect parsing operations
                    var numericPart = new string(valueStr.TakeWhile(c => char.IsDigit(c) || c == '-').ToArray());

                    if (int.TryParse(numericPart, out int count))
                        return count;
                }
            }
            return 0;
        }
    }
}