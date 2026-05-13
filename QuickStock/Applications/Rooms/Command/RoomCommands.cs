using MediatR;
using QuickStock.Domain.ITassets;
using QuickStock.Domain.Messaging;
using QuickStock.Domain.Social;
using QuickStock.Domain.Locations;
using QuickStock.Domain.Shared;
using QuickStock.Domain.Accounts;
using QuickStock.Domain.Messaging;
using QuickStock.Domain.Social;
using QuickStock.Domain.Locations;
using QuickStock.Domain.Shared;
using System.Security.Claims;

namespace QuickStock.Applications.Rooms.Command
{
    public class CreateRoomCommand : IRequest<Room>
    {
        public Room Room { get; set; }
        public ClaimsPrincipal User { get; set; }

        public CreateRoomCommand(Room room, ClaimsPrincipal user)
        {
            Room = room;
            User = user;
        }
    }

    public class UpdateRoomCommand : IRequest<bool>
    {
        public int Id { get; set; }
        public Room Room { get; set; }
        public ClaimsPrincipal User { get; set; }

        public UpdateRoomCommand(int id, Room room, ClaimsPrincipal user)
        {
            Id = id;
            Room = room;
            User = user;
        }
    }

    public class ToggleRoomStatusCommand : IRequest<object>
    {
        public int Id { get; set; }
        public ClaimsPrincipal User { get; set; }

        public ToggleRoomStatusCommand(int id, ClaimsPrincipal user)
        {
            Id = id;
            User = user;
        }
    }

    public class DeleteRoomCommand : IRequest<bool>
    {
        public int Id { get; set; }
        public ClaimsPrincipal User { get; set; }

        public DeleteRoomCommand(int id, ClaimsPrincipal user)
        {
            Id = id;
            User = user;
        }
    }
}
