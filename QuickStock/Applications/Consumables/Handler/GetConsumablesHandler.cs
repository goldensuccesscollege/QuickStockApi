using MediatR;
using Microsoft.EntityFrameworkCore;
using QuickStock.Applications.Consumables.Queries;
using QuickStock.Infrastructure.Data;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace QuickStock.Applications.Consumables.Handler
{
    public class GetConsumablesHandler : IRequestHandler<GetConsumablesQuery, ConsumableListResponse>
    {
        private readonly AppDbContext _context;

        public GetConsumablesHandler(AppDbContext context)
        {
            _context = context;
        }

        public async Task<ConsumableListResponse> Handle(GetConsumablesQuery request, CancellationToken cancellationToken)
        {
            var query = _context.ConsumableList.AsQueryable();

            if (request.CampusId.HasValue)
            {
                query = query.Where(c => c.CampusId == request.CampusId.Value);
            }

            if (!string.IsNullOrEmpty(request.SearchTerm))
            {
                var search = request.SearchTerm.ToLower();
                query = query.Where(c => c.Product.ToLower().Contains(search) || (c.Description != null && c.Description.ToLower().Contains(search)));
            }

            // Filter by stock status
            if (request.ShowOutOnly)
            {
                query = query.Where(c => (c.In - c.Out) <= 0);
            }
            else
            {
                query = query.Where(c => (c.In - c.Out) > 0);
            }

            var total = await query.CountAsync(cancellationToken);
            var consumables = await query
                .OrderByDescending(c => c.DateArrived)
                .Skip((request.Page - 1) * request.PageSize)
                .Take(request.PageSize)
                .ToListAsync(cancellationToken);

            return new ConsumableListResponse
            {
                Consumables = consumables,
                TotalCount = total
            };
        }
    }
}
