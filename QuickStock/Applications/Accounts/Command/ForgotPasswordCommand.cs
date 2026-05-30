using QuickStock.CQRS;


namespace QuickStock.Applications.Accounts.Command
{
    public class ForgotPasswordCommand : IRequest<string>
    {
        public string Email { get; set; } = string.Empty;

        public ForgotPasswordCommand() { }

        public ForgotPasswordCommand(string email)
        {
            Email = email;
        }
    }
}
