using MediatR;
using Microsoft.EntityFrameworkCore;
using QuickStock.Applications.Apparel.Queries;
using QuickStock.Infrastructure.Data;
using QuickStock.Domain.Apparel;
using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace QuickStock.Applications.Apparel.Handler
{
    public class GetSoldItemsHandler : IRequestHandler<GetSoldItemsQuery, object>
    {
        private readonly AppDbContext _context;

        public GetSoldItemsHandler(AppDbContext context)
        {
            _context = context;
        }

        public async Task<object> Handle(GetSoldItemsQuery request, CancellationToken cancellationToken)
        {
            IQueryable<ApparelItem> query = _context.ApparelItems
                .Include(i => i.ApparelType)
                .Where(i => i.Status == "Sold");

            if (request.CampusId.HasValue && request.CampusId.Value > 0)
            {
                query = query.Where(i => i.CampusId == request.CampusId.Value);
            }

            var totalItems = await query.CountAsync(cancellationToken);
            var items = await query
                .OrderByDescending(i => i.Id)
                .Skip((request.Page - 1) * request.PageSize)
                .Take(request.PageSize)
                .ToListAsync(cancellationToken);

            return new 
            { 
                TotalItems = totalItems, 
                Items = items,
                Page = request.Page,
                PageSize = request.PageSize,
                TotalPages = (int)Math.Ceiling((double)totalItems / request.PageSize)
            };
        }
    }
}
