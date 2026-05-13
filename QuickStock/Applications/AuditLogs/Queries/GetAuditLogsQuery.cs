using MediatR;
using System.Security.Claims;

namespace QuickStock.Applications.AuditLogs.Queries
{
    public class GetAuditLogsQuery : IRequest<object>
    {
        public int? CampusId { get; set; }
        public string? EntityType { get; set; }
        public int Page { get; set; } = 1;
        public int PageSize { get; set; } = 10;
        public ClaimsPrincipal User { get; set; }

        public GetAuditLogsQuery(int? campusId, string? entityType, int page, int pageSize, ClaimsPrincipal user)
        {
            CampusId = campusId;
            EntityType = entityType;
            Page = page;
            PageSize = pageSize;
            User = user;
        }
    }
}
