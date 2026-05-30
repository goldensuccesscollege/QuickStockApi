using QuickStock.CQRS;

namespace QuickStock.Applications.Accounts.Command
{
    public record VerifyAccountCommand(string Token) : IRequest<string>;
}

