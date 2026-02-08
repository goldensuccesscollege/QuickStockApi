using MediatR;


namespace QuickStock.Applications.Accounts.Command
{
    public class ForgotPasswordCommand : IRequest<string>
    {
       public string Email { get; }

        public ForgotPasswordCommand(string email)
        {
            Email = email;
        }
    }
}
