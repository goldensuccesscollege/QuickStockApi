using MediatR;
using QuickStock.Domain.Library;
using System.Security.Claims;

namespace QuickStock.Applications.Library.Command
{
    public record AddLibraryBookItemCommand(int BookId, LibraryBookItem Item, ClaimsPrincipal User) : IRequest<LibraryBookItem>;
    public record UpdateLibraryBookItemCommand(int ItemId, LibraryBookItem Item, ClaimsPrincipal User) : IRequest<bool>;
    public record DeleteLibraryBookItemCommand(int ItemId, ClaimsPrincipal User) : IRequest<bool>;
}
