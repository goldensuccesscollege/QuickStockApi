using QuickStock.CQRS;
using QuickStock.Applications.Consumables.Dto_s;
using System;

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

        public string? RequestorId { get; set; } 
        public string? RequestorName { get; set; }
        public string SubmitToken { get; set; } = string.Empty;
        public DateTime? Timestamp { get; set; }
    }
}
