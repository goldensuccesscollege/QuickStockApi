using QuickStock.CQRS;
using Microsoft.EntityFrameworkCore;
using QuickStock.Applications.Itassets.Queries;
using QuickStock.Infrastructure.Data;
using QuickStock.Domain.ITassets;
using QuickStock.Domain.Locations;
using QuickStock.Domain.Shared;
using QuickStock.Domain.Accounts;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading;
using System.Threading.Tasks;

namespace QuickStock.Applications.Itassets.Handler
{
    public class GetItAssetsHandler : IRequestHandler<GetItAssetsQuery, IEnumerable<ItAsset>>
    {
        private readonly AppDbContext _context;

        public GetItAssetsHandler(AppDbContext context)
        {
            _context = context;
        }

        public async Task<IEnumerable<ItAsset>> Handle(GetItAssetsQuery request, CancellationToken cancellationToken)
        {
            IQueryable<ItAsset> query = _context.Itassets.Include(a => a.Room);

            // If user is not Admin, hide assets in disabled rooms
            if (!request.User.IsInRole("Admin"))
            {
                query = query.Where(a => a.RoomId == null || !a.Room!.IsDisabled);
            }

            if (request.CampusId.HasValue && request.CampusId.Value > 0)
            {
                query = query.Where(a => a.CampusId == request.CampusId.Value);
            }

            if (request.RoomId.HasValue && request.RoomId.Value > 0)
            {
                query = query.Where(a => a.RoomId == request.RoomId.Value);
            }

            if (!string.IsNullOrWhiteSpace(request.SearchTerm))
            {
                var searchTerm = request.SearchTerm.Trim().ToLower();
                query = query.Where(a =>
                    a.Name.ToLower().Contains(searchTerm) ||
                    (a.Brand != null && a.Brand.ToLower().Contains(searchTerm)) ||
                    (a.Model != null && a.Model.ToLower().Contains(searchTerm)) ||
                    (a.SerialNumber != null && a.SerialNumber.ToLower().Contains(searchTerm)) ||
                    (a.Type != null && a.Type.ToLower().Contains(searchTerm)) ||
                    (a.Status != null && a.Status.ToLower().Contains(searchTerm)) ||
                    (a.Qrcode != null && a.Qrcode.ToLower().Contains(searchTerm)) ||
                    (a.Location != null && a.Location.ToLower().Contains(searchTerm)) ||
                    (a.Room != null && a.Room.RoomName.ToLower().Contains(searchTerm)));
            }

            return await query.ToListAsync(cancellationToken);
        }
    }
}
