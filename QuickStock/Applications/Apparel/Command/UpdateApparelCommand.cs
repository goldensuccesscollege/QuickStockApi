using MediatR;
using QuickStock.Domain.Apparel;
using System.Security.Claims;

namespace QuickStock.Applications.Apparel.Command
{
    public class UpdateApparelCommand : IRequest<bool>
    {
        public int Id { get; set; }
        public Appareldata Apparel { get; set; }
        public ClaimsPrincipal User { get; set; }

        public UpdateApparelCommand(int id, Appareldata apparel, ClaimsPrincipal user)
        {
            Id = id;
            Apparel = apparel;
            User = user;
        }
    }
}
