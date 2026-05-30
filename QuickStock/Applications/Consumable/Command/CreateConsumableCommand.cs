using QuickStock.CQRS;
using QuickStock.Applications.Consumables.Dto_s; // Ensure you import your response namespace

namespace QuickStock.Applications.Consumables.Commands
{

    public class CreateConsumableCommand : IRequest<ConsumableCreateResponse>
    {
        public string? ProductName { get; set; }
        public string? ProductType { get; set; }
        public int? Count { get; set; }
        public DateTime? DateArrive { get; set; }
        public int CampusId { get; set; }
    }
}