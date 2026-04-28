using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace QuickStock.Domain.ITassets
{
    public class ItAsset
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }

        [Required]
        [MaxLength(255)]
        public string Name { get; set; } = string.Empty;

        [MaxLength(255)]
        public string SerialNumber { get; set; } = string.Empty;

        [MaxLength(100)]
        public string Brand { get; set; } = string.Empty;

        [MaxLength(100)]
        public string Model { get; set; } = string.Empty;

        [MaxLength(255)]
        public string Location { get; set; } = string.Empty;

        public DateTime DateAdded { get; set; } = DateTime.UtcNow;

        [MaxLength(50)]
        public string Status { get; set; } = string.Empty; // e.g., Broken, Working, UnderMaintenance

        [MaxLength(500)]
        public string? Qrcode { get; set; }

        [MaxLength(100)]
        public string Type { get; set; } = string.Empty; // e.g., Equipment, Materials

        [Required]
        public int CampusId { get; set; }
        public Campus? Campus { get; set; }

        public int? RoomId { get; set; }
        
        [ForeignKey("RoomId")]
        public virtual Room? Room { get; set; }
    }
}
