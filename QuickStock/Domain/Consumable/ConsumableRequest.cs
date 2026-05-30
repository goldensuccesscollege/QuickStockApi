using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using QuickStock.Domain.Locations;

namespace QuickStock.Domain.Consumable
{
    [Table("ConsumableRequest")]
    public class ConsumableRequest
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }

        [Required]
        [StringLength(50)]
        public string RequestType { get; set; } = string.Empty; // "Create", "Add", "Deduct"

        [StringLength(255)]
        public string ProductName { get; set; } = string.Empty;

        [StringLength(255)]
        public string ProductType { get; set; } = string.Empty;

        public int Count { get; set; }

        public int? TargetItemId { get; set; } // Null if Create, otherwise the item ID

        [ForeignKey("TargetItemId")]
        public virtual ConsumableUnit? TargetItem { get; set; }

        [Required]
        [StringLength(50)]
        public string Status { get; set; } = "Pending"; // "Pending", "Approved", "Rejected"

        [StringLength(500)]
        public string? RejectionReason { get; set; }

        [Required]
        public DateTime Timestamp { get; set; } = DateTime.UtcNow;

        public string? RequestorId { get; set; }
        public string? RequestorName { get; set; }

        public string? ReviewerId { get; set; }
        public string? ReviewerName { get; set; }

        [Required]
        public int CampusId { get; set; }

        [ForeignKey("CampusId")]
        public virtual Campus? Campus { get; set; }
    }
}
