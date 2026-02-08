using MediatR;
using QuickStock.Applications.Accounts.Dto_s;

namespace QuickStock.Applications.Accounts.Command
{
    public record RegisterCommand(
       string Email,
       string Username,
       string Password,
       string ConfirmPassword,
       string Title,
       string FirstName,
       string LastName,
       bool IsFromApi = false // 👈 just a constructor parameter
   ) : IRequest<RegisterResponse>;
}
