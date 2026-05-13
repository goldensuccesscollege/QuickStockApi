using MediatR;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using QuickStock.Applications.AuditLogs.Queries;
using QuickStock.Infrastructure.Data;
using QuickStock.Domain.ITassets;
using QuickStock.Domain.Messaging;
using QuickStock.Domain.Social;
using QuickStock.Domain.Locations;
using QuickStock.Domain.Shared;
using QuickStock.Domain.Accounts;
using QuickStock.Domain.Messaging;
using QuickStock.Domain.Social;
using QuickStock.Domain.Locations;
using QuickStock.Domain.Shared;
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

                query = query.Where(l => allowedEntities.Contains(l.EntityType));
            }

            query = query.OrderByDescending(l => l.Timestamp);

            if (request.CampusId.HasValue && request.CampusId.Value > 0)
                query = query.Where(l => l.CampusId == request.CampusId.Value);

            if (!string.IsNullOrEmpty(request.EntityType))
                query = query.Where(l => l.EntityType == request.EntityType);

            var totalItems = await query.CountAsync(cancellationToken);
            var logs = await query.Skip((request.Page - 1) * request.PageSize).Take(request.PageSize).ToListAsync(cancellationToken);

            return new
            {
                totalItems,
                logs,
                page = request.Page,
                pageSize = request.PageSize,
                totalPages = (int)Math.Ceiling((double)totalItems / request.PageSize)
            };
        }
    }
}
