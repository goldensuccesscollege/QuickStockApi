using QuickStock.CQRS;
using QuickStock.Applications.Consumables.Dto_s;
using System.Security.Claims;

namespace QuickStock.Applications.Consumables.Commands
{
    public class ApproveConsumableRequestCommand : IRequest<ConsumableCreateResponse>
    {
        public int Id { get; set; }
        public ClaimsPrincipal User { get; set; } = null!;

        public ApproveConsumableRequestCommand(int id, ClaimsPrincipal user)
        {
            Id = id;
            User = user;
        }
    }
}
