using MediatR;
using QuickStock.Domain.Apparel;
using Microsoft.AspNetCore.Http;
using System.Security.Claims;

namespace QuickStock.Applications.Apparel.Command
{
    public class CreateApparelCommand : IRequest<Appareldata>
    {
        public Appareldata Apparel { get; set; }
        public ClaimsPrincipal User { get; set; }

        public CreateApparelCommand(Appareldata apparel, ClaimsPrincipal user)
        {
            Apparel = apparel;
            User = user;
        }
    }
}
