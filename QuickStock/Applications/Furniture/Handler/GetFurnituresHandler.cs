using QuickStock.CQRS;
using Microsoft.EntityFrameworkCore;
using QuickStock.Applications.Furniture.Queries;
using QuickStock.Infrastructure.Data;
using QuickStock.Domain.Furniture;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace QuickStock.Applications.Furniture.Handler
{
    public class GetFurnituresHandler : IRequestHandler<GetFurnituresQuery, IEnumerable<Domain.Furniture.Furniture>>
    {
        private readonly AppDbContext _context;

        public GetFurnituresHandler(AppDbContext context)
        {
            _context = context;
        }

        public async Task<IEnumerable<Domain.Furniture.Furniture>> Handle(GetFurnituresQuery request, CancellationToken cancellationToken)
        {
            var query = _context.Furnitures.AsQueryable();

            if (request.CampusId.HasValue)
            {
                query = query.Where(f => f.CampusId == request.CampusId.Value);
            }

            if (request.RoomId.HasValue)
            {
                query = query.Where(f => f.RoomId == request.RoomId.Value);
            }

            if (!string.IsNullOrWhiteSpace(request.SearchTerm))
            {
                var search = request.SearchTerm.ToLower();
                query = query.Where(f => f.Item_Name.ToLower().Contains(search) || 
                                         (f.Item_number != null && f.Item_number.ToLower().Contains(search)) || 
                                         (f.Brand != null && f.Brand.ToLower().Contains(search)) || 
                                         f.Item_ID.ToString().Contains(search));

            }

            return await query
                .Include(f => f.Room)
                .Include(f => f.Campus)
                .OrderByDescending(f => f.DateAdded)
                .ToListAsync(cancellationToken);
        }
    }
}
