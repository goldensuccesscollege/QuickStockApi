using MediatR;

namespace QuickStock.Applications.Accounts.Command
{
    public class ResetPasswordCommand : IRequest<string>
    {
        public string Email { get; set; } = string.Empty;
        public string Token { get; set; } = string.Empty;
        public string NewPassword { get; set; } = string.Empty;
    }
}
