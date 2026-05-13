using MediatR;

namespace QuickStock.Applications.Users.Command
{
    public class DeleteUserCommand : IRequest<bool>
    {
        public int Id { get; set; }

        public DeleteUserCommand(int id) => Id = id;
    }

    public class ToggleUserStatusCommand : IRequest<object>
    {
        public int Id { get; set; }

        public ToggleUserStatusCommand(int id) => Id = id;
    }

    public class AddCampusAccessCommand : IRequest<bool>
    {
        public int UserId { get; set; }
        public int CampusId { get; set; }

        public AddCampusAccessCommand(int userId, int campusId)
        {
            UserId = userId;
            CampusId = campusId;
        }
    }

    public class RemoveCampusAccessCommand : IRequest<bool>
    {
        public int UserId { get; set; }
        public int CampusId { get; set; }

        public RemoveCampusAccessCommand(int userId, int campusId)
        {
            UserId = userId;
            CampusId = campusId;
        }
    }

    public class ToggleCampusBlockCommand : IRequest<object>
    {
        public int UserId { get; set; }
        public int CampusId { get; set; }

        public ToggleCampusBlockCommand(int userId, int campusId)
        {
            UserId = userId;
            CampusId = campusId;
        }
    }

    public class ToggleITAccessCommand : IRequest<object>
    {
        public int Id { get; set; }

        public ToggleITAccessCommand(int id) => Id = id;
    }

    public class ToggleApparelAccessCommand : IRequest<object>
    {
        public int Id { get; set; }

        public ToggleApparelAccessCommand(int id) => Id = id;
    }

    public class ToggleMessageAccessCommand : IRequest<object>
    {
        public int Id { get; set; }

        public ToggleMessageAccessCommand(int id) => Id = id;
    }

    public class ToggleLibraryAccessCommand : IRequest<object>
    {
        public int Id { get; set; }

        public ToggleLibraryAccessCommand(int id) => Id = id;
    }

    public class ToggleHomeEconomicsAccessCommand : IRequest<object>
    {
        public int Id { get; set; }

        public ToggleHomeEconomicsAccessCommand(int id) => Id = id;
    }

    public class ToggleConsumablesAccessCommand : IRequest<object>
    {
        public int Id { get; set; }

        public ToggleConsumablesAccessCommand(int id) => Id = id;
    }
}
