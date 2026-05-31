using QuickStock.CQRS;
using QuickStock.Applications.Consumables.Dto_s;
using System.Collections.Generic;

namespace QuickStock.Applications.Consumables.Queries
{
    /// <summary>
    /// Requests a chronological inventory ledger for all consumables in a campus,
    /// showing IN/OUT quantities and the running balance per product.
    /// </summary>
    public class GetConsumableLedgerQuery : IRequest<List<ConsumableLedgerEntryDto>>
    {
        public int? CampusId { get; set; }
        public int? ProductId { get; set; } // Optional: filter to a single product
    }
}
