using System.ComponentModel.DataAnnotations;
using QuickStock.Domain.ITassets;
using QuickStock.Domain.Messaging;
using QuickStock.Domain.Social;
using QuickStock.Domain.Locations;
using QuickStock.Domain.Shared;
using QuickStock.Domain.Accounts;
using QuickStock.Domain.ITassets;
using QuickStock.Domain.Messaging;
using QuickStock.Domain.Social;
using QuickStock.Domain.Locations;
using QuickStock.Domain.Shared;
using QuickStock.Domain.Accounts;
using QuickStock.Domain.Messaging;
using QuickStock.Domain.Social;
using QuickStock.Domain.Locations;
using QuickStock.Domain.Shared;
using QuickStock.Domain.Accounts;
using System.ComponentModel.DataAnnotations.Schema;

namespace QuickStock.Domain.Messaging
{
    public class ChatGroupMember
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }

        public int ChatGroupId { get; set; }

        [ForeignKey(nameof(ChatGroupId))]
        public ChatGroup Group { get; set; } = null!;

        public int AccountId { get; set; }

        [ForeignKey(nameof(AccountId))]
        public Account Account { get; set; } = null!;

        public DateTime JoinedAt { get; set; } = DateTime.UtcNow;
    }
}
