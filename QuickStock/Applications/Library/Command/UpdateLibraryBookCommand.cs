using MediatR;
using QuickStock.Domain.Library;
using System.Security.Claims;

namespace QuickStock.Applications.Library.Command
{
    public class UpdateLibraryBookCommand : IRequest<bool>
    {
        public int Id { get; set; }
        public Librarydata Book { get; set; }
        public ClaimsPrincipal User { get; set; }

        public UpdateLibraryBookCommand(int id, Librarydata book, ClaimsPrincipal user)
        {
            Id = id;
            Book = book;
            User = user;
        }
    }
}
