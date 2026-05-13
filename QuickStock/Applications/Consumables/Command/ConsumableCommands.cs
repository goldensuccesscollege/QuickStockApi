using MediatR;
using QuickStock.Domain.Consumables;
using System.Security.Claims;

namespace QuickStock.Applications.Consumables.Command
{
    public record CreateConsumableCommand(ConsumableData Consumable, ClaimsPrincipal User) : IRequest<ConsumableData>;
    
    public record UpdateConsumableItemStatusCommand(int ItemId, string Status, ClaimsPrincipal User) : IRequest<bool>;
}
