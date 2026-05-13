using QuickStock.Domain.Accounts;
using QuickStock.Domain.ITassets;
using QuickStock.Domain.Messaging;
using QuickStock.Domain.Social;
using QuickStock.Domain.Locations;
using QuickStock.Domain.Shared;

namespace QuickStock.Domain.Social
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
