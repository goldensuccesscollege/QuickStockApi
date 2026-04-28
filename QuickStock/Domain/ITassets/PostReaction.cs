namespace QuickStock.Domain.ITassets
{
    public class PostReaction
    {
        public int Id { get; set; }
        public int PostId { get; set; }
        public ProfilePost Post { get; set; } = null!;
        public int AuthorAccountId { get; set; }
        public Account Author { get; set; } = null!;
        public string Type { get; set; } = "Like";
    }
}
