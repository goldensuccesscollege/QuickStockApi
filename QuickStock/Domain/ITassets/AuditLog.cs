using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace QuickStock.Domain.ITassets
{
    public class AuditLog
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }

        [Required]
        [MaxLength(100)]
        public string Action { get; set; } = string.Empty; // Add, Update, Delete, Transfer, ToggleStatus

        [Required]
        [MaxLength(100)]
        public string EntityType { get; set; } = string.Empty; // Room, ItAsset

        public int EntityId { get; set; }

        [MaxLength(255)]
        public string EntityName { get; set; } = string.Empty;

        [MaxLength(2000)]
        public string Details { get; set; } = string.Empty;

        public DateTime Timestamp { get; set; } = DateTime.UtcNow;

        [MaxLength(100)]
        public string? UserId { get; set; }

        [MaxLength(255)]
        public string? Username { get; set; }

        public int CampusId { get; set; }
    }
}
