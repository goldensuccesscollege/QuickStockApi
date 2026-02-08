using MediatR;
using QuickStock.Applications.Accounts.Dto_s;

namespace QuickStock.Applications.Accounts.Command
{
    public record LoginCommand(
      string Username,
      string Password
  ) : IRequest<LoginResponse>;
}
