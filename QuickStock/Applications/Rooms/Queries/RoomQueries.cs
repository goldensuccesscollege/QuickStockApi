using QuickStock.CQRS;
using QuickStock.Domain.ITassets;
using QuickStock.Domain.Locations;
using QuickStock.Domain.Shared;
using QuickStock.Domain.Accounts;
using System.Collections.Generic;
using System.Security.Claims;

namespace QuickStock.Applications.Rooms.Queries
{
    public class GetRoomsQuery : IRequest<IEnumerable<Room>>
    {
        public int? CampusId { get; set; }
        public ClaimsPrincipal User { get; set; }

        public GetRoomsQuery(int? campusId, ClaimsPrincipal user)
        {
            CampusId = campusId;
            User = user;
        }
    }

    public class GetRoomByIdQuery : IRequest<Room?>
    {
        public int Id { get; set; }
        public ClaimsPrincipal User { get; set; }

        public GetRoomByIdQuery(int id, ClaimsPrincipal user)
        {
            Id = id;
            User = user;
        }
    }
}
