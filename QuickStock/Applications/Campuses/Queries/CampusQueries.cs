using QuickStock.CQRS;
using QuickStock.Domain.ITassets;
using QuickStock.Domain.Locations;
using QuickStock.Domain.Shared;
using QuickStock.Domain.Accounts;
using System.Collections.Generic;

namespace QuickStock.Applications.Campuses.Queries
{
    public class GetCampusesQuery : IRequest<IEnumerable<Campus>> { }

    public class GetCampusByIdQuery : IRequest<Campus?>
    {
        public int Id { get; set; }
        public GetCampusByIdQuery(int id) => Id = id;
    }
}
