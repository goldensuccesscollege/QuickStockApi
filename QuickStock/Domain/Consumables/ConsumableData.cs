using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace QuickStock.Domain.Consumables
{
    public class ConsumableData
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public string Product { get; set; } = string.Empty;

        public string? Description { get; set; }
        public string Unit { get; set; } = "Pieces"; // e.g., Pieces, Ream, Gallon

        public DateTime DateArrived { get; set; } = DateTime.UtcNow;

        // Calculated fields or snapshots
        public int In { get; set; }
        public int Out { get; set; }
        [NotMapped]
        public int Balance { get => In - Out; set { } }

        public int CampusId { get; set; }

        // Navigation
        [System.Text.Json.Serialization.JsonIgnore]
        public ICollection<ConsumableItem> Items { get; set; } = new List<ConsumableItem>();
    }
}
