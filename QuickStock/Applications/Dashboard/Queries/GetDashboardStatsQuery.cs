using MediatR;

namespace QuickStock.Applications.Dashboard.Queries
{
    public class GetDashboardStatsQuery : IRequest<DashboardDto>
    {
        public int CampusId { get; }

        public GetDashboardStatsQuery(int campusId) => CampusId = campusId;
    }
}
