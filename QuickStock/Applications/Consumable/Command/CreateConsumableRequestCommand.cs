using QuickStock.CQRS;
using QuickStock.Applications.Consumables.Dto_s;
using System.Security.Claims;

namespace QuickStock.Applications.Consumables.Commands
{
    public class CreateConsumableRequestCommand : IRequest<ConsumableCreateResponse>
    {
        public string RequestType { get; set; } = string.Empty; // "Create", "Add", "Deduct"
        public string ProductName { get; set; } = string.Empty;
        public string ProductType { get; set; } = string.Empty;
        public int Count { get; set; }
        public int? TargetItemId { get; set; }
        public int CampusId { get; set; }

        [System.Text.Json.Serialization.JsonIgnore]
        [Microsoft.AspNetCore.Mvc.ModelBinding.BindNever]
        public ClaimsPrincipal? User { get; set; }
    }
}
