using MediatR;

namespace QuickStock.Applications.Accounts.Queries
{
    public class CheckResetTokenQuery : IRequest<bool>
    {
        public string Email { get; set; } = string.Empty;
        public string Token { get; set; } = string.Empty;

        public CheckResetTokenQuery(string email, string token)
        {
            Email = email;
            Token = token;
        }
    }
}
