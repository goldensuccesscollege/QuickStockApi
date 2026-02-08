using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace QuickStock.Domain
{
    public class Account
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }

        [MaxLength(100)]
        public string Username { get; set; } = string.Empty;

        [MaxLength(255)]
        public string Email { get; set; } = string.Empty;

        [MaxLength(255)]
        public string PasswordHash { get; set; } = string.Empty;


        // tinyint(1) in MySQL → bool in EF Core
        public bool AcceptTerms { get; set; }

        [MaxLength(255)]
        public string Role { get; set; } = string.Empty;

        [MaxLength(255)]
        public string? VerificationTokens { get; set; }

        public DateTime? Verified { get; set; }

        [MaxLength(255)]
        public string? ResetToken { get; set; }

        public DateTime? ResetTokenExpires { get; set; }

        public DateTime? PasswordReset { get; set; }

        public DateTime Created { get; set; } = DateTime.UtcNow;

        public DateTime? Updated { get; set; }

        [MaxLength(255)]
        public string Status { get; set; } = string.Empty;

        
    }
   
}
