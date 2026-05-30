using QuickStock.CQRS;
using QuickStock.Domain.ITassets;
using QuickStock.Domain.Locations;
using QuickStock.Domain.Shared;
using QuickStock.Domain.Accounts;
using System.Collections.Generic;
using System.Security.Claims;

namespace QuickStock.Applications.Itassets.Queries
{
    public class GetItAssetsQuery : IRequest<IEnumerable<ItAsset>>
    {
        public int? RoomId { get; set; }
        public int? CampusId { get; set; }
        public string? SearchTerm { get; set; }
        public ClaimsPrincipal User { get; set; }

        public GetItAssetsQuery(int? roomId, int? campusId, string? searchTerm, ClaimsPrincipal user)
        {
            RoomId = roomId;
            CampusId = campusId;
            SearchTerm = searchTerm;
            User = user;
        }
    }
}
