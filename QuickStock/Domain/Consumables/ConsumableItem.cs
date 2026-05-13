using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace QuickStock.Domain.Consumables
{
    public class ConsumableItem
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public string ItemCode { get; set; } = string.Empty;

        public string Status { get; set; } = "In Stock"; // "In Stock" or "Out"

        public DateTime? DateOut { get; set; }

        public int ConsumableDataId { get; set; }
        
        [ForeignKey("ConsumableDataId")]
        public ConsumableData ConsumableData { get; set; } = null!;
    }
}
