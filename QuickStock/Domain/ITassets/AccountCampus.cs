using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace QuickStock.Domain.ITassets
{
    public class AccountCampus
    {
        [Key]
        public int Id { get; set; }

        public int AccountId { get; set; }
        [ForeignKey("AccountId")]
        public virtual Account Account { get; set; } = null!;

        public int CampusId { get; set; }
        [ForeignKey("CampusId")]
        public virtual Campus Campus { get; set; } = null!;

        public bool IsBlocked { get; set; } = false;
    }
}
