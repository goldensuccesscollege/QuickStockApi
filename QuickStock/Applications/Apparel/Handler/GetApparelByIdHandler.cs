using MediatR;
using Microsoft.EntityFrameworkCore;
using QuickStock.Applications.Apparel.Queries;
using QuickStock.Infrastructure.Data;
using QuickStock.Domain.Apparel;
using System.Threading;
using System.Threading.Tasks;

namespace QuickStock.Applications.Apparel.Handler
{
    public class GetApparelByIdHandler : IRequestHandler<GetApparelByIdQuery, Appareldata?>
    {
        private readonly AppDbContext _context;

        public GetApparelByIdHandler(AppDbContext context)
        {
            _context = context;
        }

        public async Task<Appareldata?> Handle(GetApparelByIdQuery request, CancellationToken cancellationToken)
        {
            return await _context.ApparelList
                .Include(a => a.Campus)
                .FirstOrDefaultAsync(a => a.Apparel_ID == request.Id, cancellationToken);
        }
    }
}
