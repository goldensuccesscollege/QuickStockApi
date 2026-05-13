using MediatR;
using QuickStock.Applications.Users.Dtos;

namespace QuickStock.Applications.Users.Command
{
    public class CreateUserCommand : IRequest<object>
    {
        public CreateUserDto Dto { get; set; }

        public CreateUserCommand(CreateUserDto dto)
        {
            Dto = dto;
        }
    }
}
