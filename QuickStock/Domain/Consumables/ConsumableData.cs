using System.ComponentModel.DataAnnotations;

namespace QuickStock.Domain.Consumables
{
    public class ConsumableData
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public string Product { get; set; } = string.Empty;

        public string? Description { get; set; }

        public DateTime DateArrived { get; set; } = DateTime.UtcNow;

        // Calculated fields or snapshots
        public int In { get; set; }
        public int Out { get; set; }
        public int Balance => In - Out;

        public int CampusId { get; set; }

        // Navigation
        public ICollection<ConsumableItem> Items { get; set; } = new List<ConsumableItem>();
    }
}
