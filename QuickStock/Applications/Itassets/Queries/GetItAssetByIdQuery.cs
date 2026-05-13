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

namespace QuickStock.Applications.Itassets.Queries
{
    public class GetItAssetByIdQuery : IRequest<ItAsset?>
    {
        public int Id { get; set; }
        public ClaimsPrincipal User { get; set; }

        public GetItAssetByIdQuery(int id, ClaimsPrincipal user)
        {
            Id = id;
            User = user;
        }
    }
}
