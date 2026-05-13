using MediatR;
using Microsoft.EntityFrameworkCore;
using QuickStock.Applications.Apparel.Queries;
using QuickStock.Infrastructure.Data;
using QuickStock.Domain.Apparel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace QuickStock.Applications.Apparel.Handler
{
    public class QueryItemsHandler : IRequestHandler<QueryItemsQuery, IEnumerable<ApparelItem>>
    {
        private readonly AppDbContext _context;

        public QueryItemsHandler(AppDbContext context)
        {
            _context = context;
        }

        public async Task<IEnumerable<ApparelItem>> Handle(QueryItemsQuery request, CancellationToken cancellationToken)
        {
            IQueryable<ApparelItem> query = _context.ApparelItems.Include(i => i.ApparelType);

            if (!string.IsNullOrEmpty(request.Status))
            {
                query = query.Where(i => i.Status == request.Status);
            }

            if (request.CampusId.HasValue && request.CampusId.Value > 0)
            {
                query = query.Where(i => i.CampusId == request.CampusId.Value);
            }

            if (request.StartDate.HasValue)
            {
                query = query.Where(i => i.LastModified >= request.StartDate.Value);
            }

            if (request.EndDate.HasValue)
            {
                var endOfDate = request.EndDate.Value.Date.AddDays(1).AddTicks(-1);
                query = query.Where(i => i.LastModified <= endOfDate);
            }

            return await query.OrderByDescending(i => i.LastModified).ToListAsync(cancellationToken);
        }
    }
}
