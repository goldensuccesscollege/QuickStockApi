using QuickStock.CQRS;
using System.Security.Claims;

namespace QuickStock.Applications.Furniture.Command
{
    public record UpdateFurnitureCommand(Domain.Furniture.Furniture Furniture, ClaimsPrincipal User) : IRequest<Domain.Furniture.Furniture>;
    public record TransferFurnitureCommand(int Id, int TargetRoomId, ClaimsPrincipal User) : IRequest<object>;
}
