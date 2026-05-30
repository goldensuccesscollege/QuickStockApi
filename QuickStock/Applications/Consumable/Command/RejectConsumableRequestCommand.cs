using QuickStock.CQRS;
using QuickStock.Applications.Consumables.Dto_s;
using System.Security.Claims;

namespace QuickStock.Applications.Consumables.Commands
{
    public class RejectConsumableRequestCommand : IRequest<ConsumableCreateResponse>
    {
        public int Id { get; set; }
        public string RejectionReason { get; set; } = string.Empty;

        [System.Text.Json.Serialization.JsonIgnore]
        [Microsoft.AspNetCore.Mvc.ModelBinding.BindNever]
        public ClaimsPrincipal? User { get; set; }
    }
}
