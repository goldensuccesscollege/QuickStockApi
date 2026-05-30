using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using QuickStock.CQRS;
using QuickStock.Infrastructure.Data;         
using QuickStock.Domain.Consumable;          
using QuickStock.Applications.Consumables.Queries;
using QuickStock.Applications.Consumables.Dto_s;

namespace QuickStock.Applications.Consumable.Handler
{
    public class GetConsumablesQueryHandler : IRequestHandler<GetConsumablesQuery, List<ConsumableResponse>>
    {
        private readonly AppDbContext _context;

        public GetConsumablesQueryHandler(AppDbContext context)
        {
            _context = context;
        }

        public async Task<List<ConsumableResponse>> Handle(GetConsumablesQuery request, CancellationToken cancellationToken)
        {
            // Querying directly from your specific DbSet collection
            var query = _context.ConsumableUnits.AsNoTracking();

            // Filter by active campus context if one is specified
            if (request.CampusId.HasValue)
            {
                query = query.Where(c => c.CampusId == request.CampusId.Value);
            }

            // Project down into clean response payload objects
            return await query.Select(c => new ConsumableResponse
            {
                Id = c.Id,
                ProductName = c.ProductName,
                ProductType = c.ProductType,
                Count = c.Count,
                DateArrive = c.DateArrive,
                CampusId = c.CampusId
            }).ToListAsync(cancellationToken);
        }
    }
}