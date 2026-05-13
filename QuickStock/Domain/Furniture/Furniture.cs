using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using QuickStock.Domain.Locations;

namespace QuickStock.Domain.Furniture
{
    public class Furniture
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Item_ID { get; set; } // Unique ID of Furniture, Auto-increment

        [Required]
        [MaxLength(255)]
        public string Item_Name { get; set; } = string.Empty;

        [MaxLength(100)]
        public string? Item_number { get; set; } // Unique number of Furniture

        [MaxLength(100)]
        public string? Brand { get; set; }

        [MaxLength(255)]
        public string? Location { get; set; } // Room that furniture located

        public DateTime DateAdded { get; set; } = DateTime.UtcNow;

        [MaxLength(500)]
        public string? Condition { get; set; } // example the table have crack


        [MaxLength(500)]
        public string? Qrcode { get; set; }

        public int Item_count { get; set; } = 1; // how many items inside (e.g. spoons in a container)

        // For relationship with Room/Campus if needed
        public int? RoomId { get; set; }
        [ForeignKey("RoomId")]
        public virtual Room? Room { get; set; }
        
        public int CampusId { get; set; }
        public Campus? Campus { get; set; }

        [NotMapped]
        public int TotalItemsInRoom { get; set; }
    }
}
