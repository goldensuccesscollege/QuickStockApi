using QuickStock.CQRS;
using QuickStock.Domain.Furniture;
using System.Security.Claims;

namespace QuickStock.Applications.Furniture.Command
{
    public record CreateFurnitureCommand(Domain.Furniture.Furniture Furniture, ClaimsPrincipal User) : IRequest<Domain.Furniture.Furniture>;
}
