using System;
using QuickStock.Domain.Locations;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
 

namespace QuickStock.Domain.Consumable
{
    [Table("ConsumableUnit")]
    public class ConsumableUnit
    {
        // LABEL: Marks column as primary key and configures database auto-incrementing identity values
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        [Column("ID")]
        public int Id { get; set; }

        [StringLength(255)]
        [Column("ProductName", TypeName = "varchar(255)")]
        public string? ProductName { get; set; }
        [StringLength(255)]
        [Column("ProductType", TypeName = "varchar(255)")]
        public string? ProductType { get; set; }

        [Column("Count")]
        public int? Count { get; set; }

        [Column("DateArrive", TypeName = "datetime")]
        public DateTime? DateArrive { get; set; }

        // --- CAMPUS RELATIONSHIP START ---

        [Required]
        [Column("CampusId")] // Maps it cleanly to your database column naming scheme
        public int CampusId { get; set; }
        
        [ForeignKey("CampusId")]
        public virtual Campus? Campus { get; set; }

        // --- CAMPUS RELATIONSHIP END ---
    }
}
