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

namespace QuickStock.Applications.Campuses.Command
{
    public class CreateCampusCommand : IRequest<Campus>
    {
        public Campus Campus { get; set; }
        public CreateCampusCommand(Campus campus) => Campus = campus;
    }

    public class UpdateCampusCommand : IRequest<bool>
    {
        public int Id { get; set; }
        public Campus Campus { get; set; }
        public UpdateCampusCommand(int id, Campus campus) { Id = id; Campus = campus; }
    }

    public class DeleteCampusCommand : IRequest<bool>
    {
        public int Id { get; set; }
        public DeleteCampusCommand(int id) => Id = id;
    }
}
