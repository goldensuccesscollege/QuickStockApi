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

namespace QuickStock.Domain.Accounts
{
    public class AccountCampus
    {
        [Key]
        public int Id { get; set; }

        public int AccountId { get; set; }
        [ForeignKey("AccountId")]
        [System.Text.Json.Serialization.JsonIgnore]
        public virtual Account Account { get; set; } = null!;

        public int CampusId { get; set; }
        [ForeignKey("CampusId")]
        public virtual Campus Campus { get; set; } = null!;

        public bool IsBlocked { get; set; } = false;
    }
}
