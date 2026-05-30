using QuickStock.CQRS;
using Microsoft.EntityFrameworkCore;
using QuickStock.Applications.Itassets.Queries;
using QuickStock.Infrastructure.Data;
using QuickStock.Domain.ITassets;
using QuickStock.Domain.Locations;
using QuickStock.Domain.Shared;
using QuickStock.Domain.Accounts;
using System.Security.Claims;
using System.Threading;
using System.Threading.Tasks;

namespace QuickStock.Applications.Itassets.Handler
{
    public class GetItAssetByIdHandler : IRequestHandler<GetItAssetByIdQuery, ItAsset?>
    {
        private readonly AppDbContext _context;

        public GetItAssetByIdHandler(AppDbContext context)
        {
            _context = context;
        }

        public async Task<ItAsset?> Handle(GetItAssetByIdQuery request, CancellationToken cancellationToken)
        {
            var asset = await _context.Itassets
                .Include(a => a.Room)
                .FirstOrDefaultAsync(a => a.Id == request.Id, cancellationToken);

            if (asset == null) return null;

            // If asset is in a disabled room and user is not admin, return null (controller handles Forbid)
            if (asset.Room != null && asset.Room.IsDisabled && !request.User.IsInRole("Admin"))
                return null;

            return asset;
        }
    }
}
