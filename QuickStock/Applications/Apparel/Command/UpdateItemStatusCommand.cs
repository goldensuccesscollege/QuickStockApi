using MediatR;
using System.Security.Claims;

namespace QuickStock.Applications.Apparel.Command
{
    public class UpdateItemStatusCommand : IRequest<bool>
    {
        public int ItemId { get; set; }
        public string Status { get; set; }
        public ClaimsPrincipal User { get; set; }

        public UpdateItemStatusCommand(int itemId, string status, ClaimsPrincipal user)
        {
            ItemId = itemId;
            Status = status;
            User = user;
        }
    }
}
