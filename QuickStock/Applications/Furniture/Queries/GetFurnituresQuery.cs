using MediatR;
using QuickStock.Domain.Furniture;
using System.Collections.Generic;
using System.Security.Claims;

namespace QuickStock.Applications.Furniture.Queries
{
    public record GetFurnituresQuery(int? RoomId, int? CampusId, string? SearchTerm, ClaimsPrincipal User) : IRequest<IEnumerable<Domain.Furniture.Furniture>>;
}
