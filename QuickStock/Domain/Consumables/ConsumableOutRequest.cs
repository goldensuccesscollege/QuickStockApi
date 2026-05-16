using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace QuickStock.Domain.Consumables
{
    public class ConsumableOutRequest
    {
        [Key]
        public int Id { get; set; }

        // The specific item being requested out
        public int ConsumableItemId { get; set; }

        [System.Text.Json.Serialization.JsonIgnore]
        public ConsumableItem? ConsumableItem { get; set; }

        // Denormalized for easy display without extra joins
        public string ItemCode { get; set; } = string.Empty;
        public string ProductName { get; set; } = string.Empty;
        public int ConsumableDataId { get; set; }

        // Who requested the out
        public string RequestedByUserId { get; set; } = string.Empty;
        public string RequestedByUsername { get; set; } = string.Empty;
        public DateTime RequestedAt { get; set; } = DateTime.UtcNow;

        // Approval info
        public string Status { get; set; } = "Pending"; // Pending, Approved, Rejected
        public string? ApprovedByUserId { get; set; }
        public string? ApprovedByUsername { get; set; }
        public DateTime? ApprovedAt { get; set; }

        public string? Remarks { get; set; }
        public int CampusId { get; set; }
    }
}
