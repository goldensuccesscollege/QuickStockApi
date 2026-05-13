using MediatR;
using QuickStock.Domain.Library;
using System.Security.Claims;

namespace QuickStock.Applications.Library.Queries
{
    public class GetLibraryBookByIdQuery : IRequest<Librarydata>
    {
        public int Id { get; set; }
        public ClaimsPrincipal User { get; set; }

        public GetLibraryBookByIdQuery(int id, ClaimsPrincipal user)
        {
            Id = id;
            User = user;
        }
    }
}
