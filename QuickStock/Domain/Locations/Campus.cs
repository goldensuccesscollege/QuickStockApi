using System.ComponentModel.DataAnnotations;
using QuickStock.Domain.ITassets;
using QuickStock.Domain.Locations;
using QuickStock.Domain.Shared;
using QuickStock.Domain.Accounts;
using System.ComponentModel.DataAnnotations.Schema;

namespace QuickStock.Domain.Locations
{
    public class Campus
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int CampusId { get; set; }

        [Required]
        [MaxLength(255)]
        public string Name { get; set; } = string.Empty;

        [MaxLength(500)]
        public string? Address { get; set; }

        [MaxLength(1000)]
        public string? Description { get; set; }
    }
}
