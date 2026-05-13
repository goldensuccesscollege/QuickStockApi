using MediatR;
using System.Security.Claims;

namespace QuickStock.Applications.Itassets.Command
{
    public class DeleteItAssetCommand : IRequest<bool>
    {
        public int Id { get; set; }
        public ClaimsPrincipal User { get; set; }

        public DeleteItAssetCommand(int id, ClaimsPrincipal user)
        {
            Id = id;
            User = user;
        }
    }
}
