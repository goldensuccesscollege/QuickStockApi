using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace QuickStock.Domain.ITassets
{
    public class ProfilePost
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public int AuthorAccountId { get; set; }

        [ForeignKey("AuthorAccountId")]
        public virtual Account Author { get; set; } = null!;

        [Required]
        public string Content { get; set; } = string.Empty;

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public virtual ICollection<PostImage> Images { get; set; } = new List<PostImage>();
        public virtual ICollection<PostComment> Comments { get; set; } = new List<PostComment>();
        public virtual ICollection<PostReaction> Reactions { get; set; } = new List<PostReaction>();
    }
}
