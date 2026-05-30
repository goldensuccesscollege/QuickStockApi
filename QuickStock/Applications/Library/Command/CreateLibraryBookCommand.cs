using QuickStock.CQRS;
using QuickStock.Domain.Library;
using System.Security.Claims;

namespace QuickStock.Applications.Library.Command
{
    public class CreateLibraryBookCommand : IRequest<Librarydata>
    {
        public Librarydata Book { get; set; }
        public ClaimsPrincipal User { get; set; }

        public CreateLibraryBookCommand(Librarydata book, ClaimsPrincipal user)
        {
            Book = book;
            User = user;
        }
    }
}
