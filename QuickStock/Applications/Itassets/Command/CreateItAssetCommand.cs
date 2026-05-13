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
    public class CreateItAssetCommand : IRequest<ItAsset>
    {
        public ItAsset Asset { get; set; }
        public ClaimsPrincipal User { get; set; }

        public CreateItAssetCommand(ItAsset asset, ClaimsPrincipal user)
        {
            Asset = asset;
            User = user;
        }
    }
}
