using QuickStock.Domain.Locations;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace QuickStock.Domain.Library
{
    public class Librarydata
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int ItemId { get; set; }

        [Required]
        public string? BookNumber { get; set; }

        [Required]
        public string Title { get; set; } = string.Empty;

        public string? Subtitle { get; set; }

        [Required]
        public string? Author { get; set; }

        public string? CoAuthor { get; set; }

        public string? Class { get; set; }

        public string? Publisher { get; set; }

        [Required]
        public int? Year { get; set; }

        public string? Edition { get; set; }

        [Required]
        public string? Volumes { get; set; }

        [Required]
        public int? Pages { get; set; }

        public string? ISBN { get; set; }

        [Required]
        public DateTime? DateReceived { get; set; }

        public decimal? CostPrice { get; set; }

        public string? SourceOfFund { get; set; }

        public string? Donor { get; set; }

        public string? AcquisitionType { get; set; }

        public string? Remarks { get; set; }

        public string? Genre { get; set; }

        public string? Language { get; set; }

        public string? ShelfLocation { get; set; }

        public string? CallNumber { get; set; }

        public string? CoverImage { get; set; }

        public DateTime CreatedAt { get; set; }

        public DateTime UpdatedAt { get; set; }

        public int CampusId { get; set; }
        public Campus? Campus { get; set; }

        // Navigation property for individual copies
        public virtual ICollection<LibraryBookItem> Items { get; set; } = new List<LibraryBookItem>();

        [NotMapped]
        public string? InitialAccessionNumber { get; set; }
    }
}