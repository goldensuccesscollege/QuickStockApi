using QuickStock.CQRS;
using QuickStock.Domain.Apparel;
using System.Collections.Generic;

namespace QuickStock.Applications.Apparel.Queries
{
    public class GetApparelItemsQuery : IRequest<IEnumerable<ApparelItem>>
    {
        public int ApparelId { get; set; }

        public GetApparelItemsQuery(int apparelId)
        {
            ApparelId = apparelId;
        }
    }
}
