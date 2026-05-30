using QuickStock.CQRS;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using QuickStock.Applications.AuditLogs.Queries;
using QuickStock.Infrastructure.Data;
using QuickStock.Domain.ITassets;
using QuickStock.Domain.Locations;
using QuickStock.Domain.Shared;
using QuickStock.Domain.Accounts;
using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace QuickStock.Applications.AuditLogs.Handler
{
    public class GetAuditLogsHandler : IRequestHandler<GetAuditLogsQuery, object>
    {
        private readonly AppDbContext _context;

        public GetAuditLogsHandler(AppDbContext context) => _context = context;

        public async Task<object> Handle(GetAuditLogsQuery request, CancellationToken cancellationToken)
        {
            var isAdmin = request.User.IsInRole("Admin");
            var isLibraryAdmin = request.User.IsInRole("Library Admin");
            var isHEAdmin = request.User.IsInRole("Home Economics Admin");
            
            var canAccessIT = isAdmin || (request.User.FindFirst("CanAccessITAssets")?.Value == "True");
            var canAccessApparel = isAdmin || (request.User.FindFirst("CanAccessApparel")?.Value == "True");
            var canAccessLibrary = isAdmin || isLibraryAdmin || (request.User.FindFirst("CanAccessLibrary")?.Value == "True");
            var canAccessHE = isAdmin || isHEAdmin || (request.User.FindFirst("CanAccessHomeEconomics")?.Value == "True");

            IQueryable<AuditLog> query = _context.AuditLogs;

            if (!isAdmin)
            {
                var allowedEntities = new List<string>();
                if (canAccessIT) allowedEntities.AddRange(new[] { "ItAsset", "Room" });
                if (canAccessApparel) allowedEntities.Add("Apparel");
                if (canAccessLibrary) allowedEntities.Add("Library");
                if (canAccessHE) allowedEntities.Add("Furniture");
                allowedEntities.Add("Consumable");

                query = query.Where(l => allowedEntities.Contains(l.EntityType));
            }

            query = query.OrderByDescending(l => l.Timestamp);

            if (request.CampusId.HasValue && request.CampusId.Value > 0)
                query = query.Where(l => l.CampusId == request.CampusId.Value);

            if (!string.IsNullOrEmpty(request.EntityType))
                query = query.Where(l => l.EntityType == request.EntityType);

            var totalItems = await query.CountAsync(cancellationToken);
            var logs = await query.Skip((request.Page - 1) * request.PageSize).Take(request.PageSize).ToListAsync(cancellationToken);

            var userIds = logs.Where(l => !string.IsNullOrEmpty(l.UserId)).Select(l => l.UserId!).Distinct().ToList();
            var accountIds = new List<int>();
            foreach (var uid in userIds)
            {
                if (int.TryParse(uid, out int id))
                {
                    accountIds.Add(id);
                }
            }

            var accounts = await _context.Accounts
                .Include(a => a.Profile)
                .Where(a => accountIds.Contains(a.Id))
                .ToListAsync(cancellationToken);

            var logsResult = logs.Select(l => {
                var displayUser = l.Username;
                if (!string.IsNullOrEmpty(l.UserId) && int.TryParse(l.UserId, out int accountId))
                {
                    var acc = accounts.FirstOrDefault(a => a.Id == accountId);
                    if (acc != null && acc.Profile != null && !string.IsNullOrEmpty(acc.Profile.FirstName))
                    {
                        displayUser = acc.Profile.FirstName + (string.IsNullOrEmpty(acc.Profile.LastName) ? "" : " " + acc.Profile.LastName);
                    }
                }
                return new {
                    l.Id,
                    l.Action,
                    l.EntityType,
                    l.EntityId,
                    l.EntityName,
                    l.Details,
                    l.Timestamp,
                    l.UserId,
                    Username = displayUser,
                    l.CampusId,
                    l.Status
                };
            }).ToList();

            return new
            {
                totalItems,
                logs = logsResult,
                page = request.Page,
                pageSize = request.PageSize,
                totalPages = (int)Math.Ceiling((double)totalItems / request.PageSize)
            };
        }
    }
}
