using QuickStock.CQRS;
using QuickStock.Domain.Apparel;
using System;
using System.Collections.Generic;

namespace QuickStock.Applications.Apparel.Queries
{
    public class QueryItemsQuery : IRequest<IEnumerable<ApparelItem>>
    {
        public string? Status { get; set; }
        public DateTime? StartDate { get; set; }
        public DateTime? EndDate { get; set; }
        public int? CampusId { get; set; }

        public QueryItemsQuery(string? status, DateTime? startDate, DateTime? endDate, int? campusId)
        {
            Status = status;
            StartDate = startDate;
            EndDate = endDate;
            CampusId = campusId;
        }
    }
}
