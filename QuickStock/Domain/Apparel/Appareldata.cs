using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using QuickStock.Domain.ITassets;

namespace QuickStock.Domain.Apparel
{
    public class Appareldata
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Apparel_ID { get; set; }

        [Required]
        [MaxLength(255)]
        public string Apparel_Name { get; set; } = string.Empty;

        [MaxLength(100)]
        public string Category { get; set; } = string.Empty;

        [MaxLength(50)]
        public string Gender { get; set; } = string.Empty;

        [MaxLength(50)]
        public string Size { get; set; } = string.Empty;

        [MaxLength(100)]
        public string Grade_Level { get; set; } = string.Empty;

        public int Quality_In_Stock { get; set; }

        public int Reorder_level { get; set; }

        public DateTime Date_Purchased { get; set; }

        [Column(TypeName = "decimal(18,2)")]
        public decimal Unit_Price { get; set; }

        [MaxLength(255)]
        public string Supplier_Name { get; set; } = string.Empty;

        [MaxLength(500)]
        public string? Remarks { get; set; }

        [Required]
        public int CampusId { get; set; }
        
        [ForeignKey("CampusId")]
        public virtual Campus? Campus { get; set; }

        public virtual ICollection<ApparelItem> Items { get; set; } = new List<ApparelItem>();
    }
}
