using MediatR;
using QuickStock.Domain.Consumables;
using System.Collections.Generic;

namespace QuickStock.Applications.Consumables.Queries
{
    public record GetConsumablesQuery(int? CampusId, string? SearchTerm, int Page, int PageSize, bool ShowOutOnly = false) : IRequest<ConsumableListResponse>;
    
    public record GetConsumableByIdQuery(int Id) : IRequest<ConsumableData?>;
    
    public record GetConsumableItemsQuery(int ConsumableId, bool ShowOutOnly = false) : IRequest<List<ConsumableItemResponseDto>>;
    
    public class ConsumableItemResponseDto
    {
        public int Id { get; set; }
        public string ItemCode { get; set; } = string.Empty;
        public string Status { get; set; } = string.Empty;
        public DateTime? DateOut { get; set; }
        public int ConsumableDataId { get; set; }
        public string? AddedByUsername { get; set; }
        
        // Request Info
        public bool IsRequestPending { get; set; }
        public int? PendingRequestId { get; set; }
        public string? RequestedByUserId { get; set; }
        public string? RequestedByUsername { get; set; }
        public DateTime? RequestedAt { get; set; }

        public string? ApprovedByUsername { get; set; }
        public DateTime? ApprovedAt { get; set; }
    }

    public record GetPendingOutRequestsQuery(int CampusId) : IRequest<List<ConsumableOutRequest>>;
    public record GetOutRequestHistoryQuery(int CampusId, string? Status = null, string? RequestedByUserId = null) : IRequest<List<ConsumableOutRequest>>;

    public class ConsumableListResponse
    {
        public List<ConsumableData> Consumables { get; set; } = new();
        public int TotalCount { get; set; }
    }
}
