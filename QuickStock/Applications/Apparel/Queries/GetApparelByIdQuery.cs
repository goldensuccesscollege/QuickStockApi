using MediatR;
using QuickStock.Domain.Apparel;

namespace QuickStock.Applications.Apparel.Queries
{
    public class GetApparelByIdQuery : IRequest<Appareldata?>
    {
        public int Id { get; set; }

        public GetApparelByIdQuery(int id)
        {
            Id = id;
        }
    }
}
