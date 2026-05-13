using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using QuickStock.Domain.Locations;

namespace QuickStock.Domain.Library
{
    public class LibraryBookItem
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }

        [Required]
        public int LibrarydataId { get; set; }

        [ForeignKey("LibrarydataId")]
        [System.Text.Json.Serialization.JsonIgnore]
        public virtual Librarydata? Book { get; set; }

        [Required]
        public string AccessionNumber { get; set; } = string.Empty;

        public string? QRCode { get; set; }

        public string Status { get; set; } = "Available"; // Available, Checked Out, Reserved, Lost, Damaged

        public string? Condition { get; set; }

        public DateTime? DateAcquired { get; set; }

        [Required]
        public int CampusId { get; set; }

        [ForeignKey("CampusId")]
        public virtual Campus? Campus { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
    }
}
