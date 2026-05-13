using MediatR;
using Microsoft.EntityFrameworkCore;
using QuickStock.Applications.Apparel.Queries;
using QuickStock.Infrastructure.Data;
using QuickStock.Domain.Apparel;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace QuickStock.Applications.Apparel.Handler
{
    public class GetApparelItemsHandler : IRequestHandler<GetApparelItemsQuery, IEnumerable<ApparelItem>>
    {
        private readonly AppDbContext _context;

        public GetApparelItemsHandler(AppDbContext context)
        {
            _context = context;
        }

        public async Task<IEnumerable<ApparelItem>> Handle(GetApparelItemsQuery request, CancellationToken cancellationToken)
        {
            return await _context.ApparelItems
                .Where(i => i.AppareldataId == request.ApparelId)
                .ToListAsync(cancellationToken);
        }
    }
}
