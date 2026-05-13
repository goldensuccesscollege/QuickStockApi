using MediatR;
using QuickStock.Domain.Apparel;

namespace QuickStock.Applications.Apparel.Queries
{
    public class GetSoldItemsQuery : IRequest<object>
    {
        public int? CampusId { get; set; }
        public int Page { get; set; } = 1;
        public int PageSize { get; set; } = 10;

        public GetSoldItemsQuery(int? campusId, int page, int pageSize)
        {
            CampusId = campusId;
            Page = page;
            PageSize = pageSize;
        }
    }
}
