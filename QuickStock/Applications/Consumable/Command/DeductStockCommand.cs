using QuickStock.CQRS;
using QuickStock.Applications.Consumables.Dto_s;

namespace QuickStock.Applications.Consumables.Commands
{
    public class DeductStockCommand : IRequest<ConsumableCreateResponse>
    {
        public int Id { get; set; }
        public int Quantity { get; set; }
    }
}