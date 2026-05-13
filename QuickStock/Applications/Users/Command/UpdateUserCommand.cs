using MediatR;
using QuickStock.Applications.Users.Dtos;

namespace QuickStock.Applications.Users.Command
{
    public class UpdateUserCommand : IRequest<bool>
    {
        public int Id { get; set; }
        public UpdateUserDto Dto { get; set; }

        public UpdateUserCommand(int id, UpdateUserDto dto)
        {
            Id = id;
            Dto = dto;
        }
    }
}
