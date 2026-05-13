using MediatR;
using System.Security.Claims;

namespace QuickStock.Applications.Apparel.Command
{
    public class DeleteApparelCommand : IRequest<bool>
    {
        public int Id { get; set; }
        public ClaimsPrincipal User { get; set; }

        public DeleteApparelCommand(int id, ClaimsPrincipal user)
        {
            Id = id;
            User = user;
        }
    }
}
