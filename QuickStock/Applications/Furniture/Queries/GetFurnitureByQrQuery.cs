using QuickStock.CQRS;
using QuickStock.Domain.Furniture;

namespace QuickStock.Applications.Furniture.Queries
{
    public record GetFurnitureByQrQuery(string QrCode) : IRequest<Domain.Furniture.Furniture?>;
}
