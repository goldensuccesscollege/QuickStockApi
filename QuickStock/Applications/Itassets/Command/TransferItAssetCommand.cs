using MediatR;
using System.Security.Claims;

namespace QuickStock.Applications.Itassets.Command
{
    public class TransferItAssetCommand : IRequest<object>
    {
        public int Id { get; set; }
        public int TargetRoomId { get; set; }
        public ClaimsPrincipal User { get; set; }

        public TransferItAssetCommand(int id, int targetRoomId, ClaimsPrincipal user)
        {
            Id = id;
            TargetRoomId = targetRoomId;
            User = user;
        }
    }
}
