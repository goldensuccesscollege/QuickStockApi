using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace QuickStock.Domain.ITassets
{
    public class ChatMessage
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }

        public int SenderAccountId { get; set; }

        [ForeignKey(nameof(SenderAccountId))]
        public Account Sender { get; set; } = null!;

        public int? ReceiverAccountId { get; set; }

        [ForeignKey(nameof(ReceiverAccountId))]
        public Account? Receiver { get; set; }

        public int? GroupId { get; set; }

        [ForeignKey(nameof(GroupId))]
        public ChatGroup? Group { get; set; }

        [Required]
        public string Message { get; set; } = string.Empty;

        public DateTime Timestamp { get; set; } = DateTime.UtcNow;
    }
}
