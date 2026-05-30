using QuickStock.CQRS;
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
    public class GetApparelHandler : IRequestHandler<GetApparelQuery, object>
    {
        private readonly AppDbContext _context;

        public GetApparelHandler(AppDbContext context)
        {
            _context = context;
        }

        public async Task<object> Handle(GetApparelQuery request, CancellationToken cancellationToken)
        {
            IQueryable<Appareldata> query = _context.ApparelList.Include(a => a.Campus);

            if (request.CampusId.HasValue && request.CampusId.Value > 0)
            {
                query = query.Where(a => a.CampusId == request.CampusId.Value);
            }

            if (!string.IsNullOrWhiteSpace(request.SearchTerm))
            {
                var searchTerm = request.SearchTerm.Trim().ToLower();
                query = query.Where(a => 
                    a.Apparel_Name.ToLower().Contains(searchTerm) || 
                    a.Category.ToLower().Contains(searchTerm) ||
                    a.Supplier_Name.ToLower().Contains(searchTerm));
            }

            var totalItems = await query.CountAsync(cancellationToken);
            var apparel = await query
                .Skip((request.Page - 1) * request.PageSize)
                .Take(request.PageSize)
                .ToListAsync(cancellationToken);

            return new 
            { 
                TotalItems = totalItems, 
                Apparel = apparel,
                Page = request.Page,
                PageSize = request.PageSize,
                TotalPages = (int)Math.Ceiling((double)totalItems / request.PageSize)
            };
        }
    }
}
