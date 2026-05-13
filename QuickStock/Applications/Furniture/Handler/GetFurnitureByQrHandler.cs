using MediatR;
using Microsoft.EntityFrameworkCore;
using QuickStock.Applications.Furniture.Queries;
using QuickStock.Infrastructure.Data;
using QuickStock.Domain.Furniture;
using System.Threading;
using System.Threading.Tasks;

namespace QuickStock.Applications.Furniture.Handler
{
    public class GetFurnitureByQrHandler : IRequestHandler<GetFurnitureByQrQuery, Domain.Furniture.Furniture?>
    {
        private readonly AppDbContext _context;

        public GetFurnitureByQrHandler(AppDbContext context)
        {
            _context = context;
        }

        public async Task<Domain.Furniture.Furniture?> Handle(GetFurnitureByQrQuery request, CancellationToken cancellationToken)
        {
            var furniture = await _context.Furnitures
                .Include(f => f.Room)
                .Include(f => f.Campus)
                .FirstOrDefaultAsync(f => f.Qrcode == request.QrCode || f.Item_number == request.QrCode, cancellationToken);

            if (furniture != null && furniture.RoomId.HasValue)
            {
                furniture.TotalItemsInRoom = await _context.Furnitures
                    .CountAsync(f => f.RoomId == furniture.RoomId, cancellationToken);
            }

            return furniture;
        }
    }
}
