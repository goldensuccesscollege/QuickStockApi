using MediatR;
using QuickStock.Domain.ITassets;
using QuickStock.Domain.Messaging;
using QuickStock.Domain.Social;
using QuickStock.Domain.Locations;
using QuickStock.Domain.Shared;
using QuickStock.Domain.Accounts;
using QuickStock.Domain.Messaging;
using QuickStock.Domain.Social;
using QuickStock.Domain.Locations;
using QuickStock.Domain.Shared;
using System.Security.Claims;

namespace QuickStock.Applications.Itassets.Command
{
    public class UpdateItAssetCommand : IRequest<bool>
    {
        public int Id { get; set; }
        public ItAsset Asset { get; set; }
        public ClaimsPrincipal User { get; set; }

        public UpdateItAssetCommand(int id, ItAsset asset, ClaimsPrincipal user)
        {
            Id = id;
            Asset = asset;
            User = user;
        }
    }
}
