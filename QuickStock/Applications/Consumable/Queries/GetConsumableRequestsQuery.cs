using QuickStock.CQRS;
using QuickStock.Applications.Consumables.Dto_s;
using System.Collections.Generic;

namespace QuickStock.Applications.Consumables.Queries
{
    public class GetConsumableRequestsQuery : IRequest<List<ConsumableRequestDto>>
    {
        public int? CampusId { get; set; }
        public string? Status { get; set; } // "Pending", "Approved", "Rejected"
    }
}
