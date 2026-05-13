using MediatR;
using QuickStock.Domain.Apparel;

namespace QuickStock.Applications.Apparel.Queries
{
    public class GetApparelQuery : IRequest<object>
    {
        public int? CampusId { get; set; }
        public string? SearchTerm { get; set; }
        public int Page { get; set; } = 1;
        public int PageSize { get; set; } = 5;

        public GetApparelQuery(int? campusId, string? searchTerm, int page, int pageSize)
        {
            CampusId = campusId;
            SearchTerm = searchTerm;
            Page = page;
            PageSize = pageSize;
        }
    }
}
