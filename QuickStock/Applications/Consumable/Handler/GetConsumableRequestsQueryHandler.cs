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
    public class GetConsumableRequestsQueryHandler : IRequestHandler<GetConsumableRequestsQuery, List<ConsumableRequestDto>>
    {
        private readonly AppDbContext _context;

        public GetConsumableRequestsQueryHandler(AppDbContext context)
        {
            _context = context;
        }

        public async Task<List<ConsumableRequestDto>> Handle(GetConsumableRequestsQuery request, CancellationToken cancellationToken)
        {
            var query = _context.ConsumableRequests.AsNoTracking();

            if (request.CampusId.HasValue && request.CampusId.Value > 0)
            {
                query = query.Where(c => c.CampusId == request.CampusId.Value);
            }

            if (!string.IsNullOrEmpty(request.Status))
            {
                query = query.Where(c => c.Status == request.Status);
            }

            return await query.Select(c => new ConsumableRequestDto
            {
                Id = c.Id,
                RequestType = c.RequestType,
                ProductName = c.ProductName,
                ProductType = c.ProductType,
                Count = c.Count,
                TargetItemId = c.TargetItemId,
                Status = c.Status,
                RejectionReason = c.RejectionReason,
                Timestamp = c.Timestamp,
                RequestorId = c.RequestorId,
                RequestorName = c.RequestorName,
                ReviewerId = c.ReviewerId,
                ReviewerName = c.ReviewerName,
                CampusId = c.CampusId
            }).OrderByDescending(c => c.Timestamp).ToListAsync(cancellationToken);
        }
    }
}
