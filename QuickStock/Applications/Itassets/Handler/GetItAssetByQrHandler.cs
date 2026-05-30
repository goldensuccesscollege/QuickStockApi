using QuickStock.CQRS;
using Microsoft.EntityFrameworkCore;
using QuickStock.Applications.Itassets.Queries;
using QuickStock.Infrastructure.Data;
using QuickStock.Domain.ITassets;
using QuickStock.Domain.Locations;
using QuickStock.Domain.Shared;
using QuickStock.Domain.Accounts;
using System.Threading;
using System.Threading.Tasks;

namespace QuickStock.Applications.Itassets.Handler
{
    public class GetItAssetByQrHandler : IRequestHandler<GetItAssetByQrQuery, ItAsset?>
    {
        private readonly AppDbContext _context;

        public GetItAssetByQrHandler(AppDbContext context)
        {
            _context = context;
        }

        public async Task<ItAsset?> Handle(GetItAssetByQrQuery request, CancellationToken cancellationToken)
        {
            var asset = await _context.Itassets
                .Include(a => a.Room)
                .Include(a => a.Campus)
                .FirstOrDefaultAsync(a => a.Qrcode == request.QrCode, cancellationToken);

            if (asset != null && asset.RoomId.HasValue)
            {
                asset.TotalItemsInRoom = await _context.Itassets
                    .CountAsync(a => a.RoomId == asset.RoomId, cancellationToken);
            }

            return asset;
        }
    }
}
