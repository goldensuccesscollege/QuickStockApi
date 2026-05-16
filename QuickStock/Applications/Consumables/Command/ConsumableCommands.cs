using MediatR;
using QuickStock.Domain.Consumables;
using System.Security.Claims;

namespace QuickStock.Applications.Consumables.Command
{
    public record CreateConsumableCommand(ConsumableData Consumable, ClaimsPrincipal User) : IRequest<ConsumableData>;
    
    public record UpdateConsumableItemStatusCommand(int ItemId, string Status, ClaimsPrincipal User) : IRequest<bool>;

    public record RequestItemOutCommand(int ItemId, ClaimsPrincipal User) : IRequest<(bool Success, string Message)>;

    public record ApproveOutRequestCommand(int RequestId, ClaimsPrincipal Approver) : IRequest<(bool Success, string Message)>;

    public record RejectOutRequestCommand(int RequestId, string Reason, ClaimsPrincipal Approver) : IRequest<(bool Success, string Message)>;

    public record CancelOutRequestCommand(int RequestId, ClaimsPrincipal User) : IRequest<(bool Success, string Message)>;

    public record UpdateConsumableCommand(ConsumableData Consumable, ClaimsPrincipal User) : IRequest<bool>;

    public record RestockConsumableCommand(int ConsumableId, int QuantityToAdd, ClaimsPrincipal User) : IRequest<bool>;
}
