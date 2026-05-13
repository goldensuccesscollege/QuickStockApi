using MediatR;
using QuickStock.Domain.Consumables;
using System.Collections.Generic;

namespace QuickStock.Applications.Consumables.Queries
{
    public record GetConsumablesQuery(int? CampusId, string? SearchTerm, int Page, int PageSize) : IRequest<ConsumableListResponse>;
    
    public record GetConsumableByIdQuery(int Id) : IRequest<ConsumableData?>;
    
    public record GetConsumableItemsQuery(int ConsumableId) : IRequest<List<ConsumableItem>>;

    public class ConsumableListResponse
    {
        public List<ConsumableData> Consumables { get; set; } = new();
        public int TotalCount { get; set; }
    }
}
