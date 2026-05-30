namespace QuickStock.Applications.Consumables.Dto_s
{
    public class ConsumableCreateResponse
    {
        public int Id { get; set; }
        public string Message { get; set; } = "Added successfully";
    }
    public class ConsumableResponse
    {
        public int Id { get; set; }
        public string? ProductName { get; set; }
        public string? ProductType { get; set; }
        public int? Count { get; set; }
        public DateTime? DateArrive { get; set; }
        public int CampusId { get; set; }
        public string? CampusName { get; set; } // Helpful if you join tables
    }

    public class ConsumableRequestDto
    {
        public int Id { get; set; }
        public string RequestType { get; set; } = string.Empty;
        public string ProductName { get; set; } = string.Empty;
        public string ProductType { get; set; } = string.Empty;
        public int Count { get; set; }
        public int? TargetItemId { get; set; }
        public string Status { get; set; } = "Pending";
        public string? RejectionReason { get; set; }
        public DateTime Timestamp { get; set; }
        public string? RequestorId { get; set; }
        public string? RequestorName { get; set; }
        public string? ReviewerId { get; set; }
        public string? ReviewerName { get; set; }
        public int CampusId { get; set; }
    }
}