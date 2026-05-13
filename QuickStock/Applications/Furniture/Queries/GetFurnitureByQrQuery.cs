using MediatR;
using QuickStock.Domain.Furniture;

namespace QuickStock.Applications.Furniture.Queries
{
    public record GetFurnitureByQrQuery(string QrCode) : IRequest<Domain.Furniture.Furniture?>;
}
