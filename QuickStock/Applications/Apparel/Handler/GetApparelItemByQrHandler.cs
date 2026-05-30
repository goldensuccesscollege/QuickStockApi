using QuickStock.CQRS;
using Microsoft.EntityFrameworkCore;
using QuickStock.Applications.Apparel.Queries;
using QuickStock.Infrastructure.Data;
using QuickStock.Domain.Apparel;
using System.Threading;
using System.Threading.Tasks;

namespace QuickStock.Applications.Apparel.Handler
{
    public class GetApparelItemByQrHandler : IRequestHandler<GetApparelItemByQrQuery, ApparelItem?>
    {
        private readonly AppDbContext _context;

        public GetApparelItemByQrHandler(AppDbContext context)
        {
            _context = context;
        }

        public async Task<ApparelItem?> Handle(GetApparelItemByQrQuery request, CancellationToken cancellationToken)
        {
            return await _context.ApparelItems
                .Include(i => i.ApparelType)
                .Include(i => i.Campus)
                .FirstOrDefaultAsync(i => i.Apparel_Number == request.Qr, cancellationToken);
        }
    }
}
