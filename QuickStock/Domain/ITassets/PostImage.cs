using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace QuickStock.Domain.ITassets
{
    public class PostImage
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public int PostId { get; set; }

        [ForeignKey("PostId")]
        public virtual ProfilePost Post { get; set; } = null!;

        [Required]
        public string ImagePath { get; set; } = string.Empty;
    }
}
