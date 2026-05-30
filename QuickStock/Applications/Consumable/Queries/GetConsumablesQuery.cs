using QuickStock.CQRS;
using QuickStock.Applications.Consumables.Dto_s;
using System.Collections.Generic;

namespace QuickStock.Applications.Consumables.Queries
{
    // Requests a list of consumables back
    public class GetConsumablesQuery : IRequest<List<ConsumableResponse>>
    {
        public int? CampusId { get; set; } // Optional filter if user is locked to a campus
    }
}