using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using QuickStock.Domain.ITassets;

using System.Text.Json.Serialization;

namespace QuickStock.Domain.Apparel
{
    public class ApparelItem
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }

        [Required]
        public int AppareldataId { get; set; }

        [JsonIgnore]
        [ForeignKey("AppareldataId")]
        public virtual Appareldata? ApparelType { get; set; }

        [Required]
        [MaxLength(100)]
        public string Apparel_Number { get; set; } = string.Empty;

        [Required]
        [MaxLength(50)]
        public string Status { get; set; } = "In Stock"; // In Stock, Issued, Damaged, Lost

        [Required]
        public int CampusId { get; set; }

        [ForeignKey("CampusId")]
        public virtual Campus? Campus { get; set; }

        public DateTime DateCreated { get; set; } = DateTime.UtcNow;
    }
}
