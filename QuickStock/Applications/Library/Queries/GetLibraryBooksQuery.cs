using QuickStock.CQRS;
using QuickStock.Domain.Library;
using System.Security.Claims;
using System.Collections.Generic;

namespace QuickStock.Applications.Library.Queries
{
    public class GetLibraryBooksQuery : IRequest<IEnumerable<Librarydata>>
    {
        public int? CampusId { get; set; }
        public ClaimsPrincipal User { get; set; }

        public GetLibraryBooksQuery(int? campusId, ClaimsPrincipal user)
        {
            CampusId = campusId;
            User = user;
        }
    }
}
