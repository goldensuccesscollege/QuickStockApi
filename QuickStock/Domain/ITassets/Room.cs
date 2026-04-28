using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace QuickStock.Domain.ITassets
{
    public class Room
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int RoomId { get; set; }

        [Required]
        [MaxLength(255)]
        public string RoomName { get; set; } = string.Empty;

        [Required]
        [MaxLength(100)]
        public string RoomFloor { get; set; } = string.Empty;

        [Required]
        public int CampusId { get; set; }
        public Campus? Campus { get; set; }

        [MaxLength(1000)]
        public string? RoomDescription { get; set; }

        public bool IsDisabled { get; set; } = false;
    }
}
