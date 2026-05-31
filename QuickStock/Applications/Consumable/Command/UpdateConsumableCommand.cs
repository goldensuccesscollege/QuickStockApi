using QuickStock.CQRS;
using QuickStock.Applications.Consumables.Dto_s;

namespace QuickStock.Applications.Consumables.Commands
{
    public class UpdateConsumableCommand : IRequest<ConsumableCreateResponse>
    {
        public int Id { get; set; }
        public string? ProductName { get; set; }
        public string? ProductType { get; set; }
        public int? Count { get; set; }
        public DateTime? DateArrive { get; set; }
    }
}
