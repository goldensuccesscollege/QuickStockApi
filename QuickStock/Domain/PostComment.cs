using System;
using QuickStock.Domain;

namespace QuickStock.Domain
{
    public class PostComment
    {
        public int Id { get; set; }
        public int PostId { get; set; }
        public ProfilePost Post { get; set; } = null!;
        public int AuthorAccountId { get; set; }
        public Account Author { get; set; } = null!;
        public string Content { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }
}
