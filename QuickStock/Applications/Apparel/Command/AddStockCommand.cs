using QuickStock.CQRS;
using System.Security.Claims;

namespace QuickStock.Applications.Apparel.Command
{
    public class AddStockCommand : IRequest<object>
    {
        public int Id { get; set; }
        public int AdditionalQuantity { get; set; }
        public ClaimsPrincipal User { get; set; }

        public AddStockCommand(int id, int additionalQuantity, ClaimsPrincipal user)
        {
            Id = id;
            AdditionalQuantity = additionalQuantity;
            User = user;
        }
    }
}
