using MediatR;
using Microsoft.EntityFrameworkCore;
using QuickStock.Applications.Dashboard.Queries;
using QuickStock.Infrastructure.Data;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace QuickStock.Applications.Dashboard.Handler
{
    public class GetDashboardStatsHandler : IRequestHandler<GetDashboardStatsQuery, DashboardDto>
    {
        private readonly AppDbContext _context;

        public GetDashboardStatsHandler(AppDbContext context) => _context = context;

        public async Task<DashboardDto> Handle(GetDashboardStatsQuery request, CancellationToken cancellationToken)
        {
            var campusId = request.CampusId;
            var dto = new DashboardDto();

            try
            {
                // IT Assets stats
                dto.TotalAssets = await _context.Itassets.CountAsync(a => a.CampusId == campusId, cancellationToken);
                dto.RoomCount = await _context.Rooms.CountAsync(r => r.CampusId == campusId, cancellationToken);
                dto.DisabledRooms = await _context.Rooms.CountAsync(r => r.CampusId == campusId && r.IsDisabled, cancellationToken);

                dto.AssetsByStatus = await _context.Itassets
                    .Where(a => a.CampusId == campusId)
                    .GroupBy(a => a.Status)
                    .Select(g => new StatusCountDto { Status = g.Key ?? "Unknown", Count = g.Count() })
                    .ToListAsync(cancellationToken);

                dto.AssetsByType = await _context.Itassets
                    .Where(a => a.CampusId == campusId)
                    .GroupBy(a => a.Type)
                    .Select(g => new TypeCountDto { Type = g.Key ?? "Unknown", Count = g.Count() })
                    .ToListAsync(cancellationToken);

                // Recent activity (IT Assets)
                var recentItLogsRaw = await _context.AuditLogs
                    .Where(l => l.CampusId == campusId && l.EntityType == "ItAsset")
                    .OrderByDescending(l => l.Timestamp)
                    .Take(5)
                    .ToListAsync(cancellationToken);

                dto.RecentItActivities = recentItLogsRaw.Select(l => new RecentActivityDto
                {
                    Action = l.Action,
                    EntityName = l.EntityName,
                    EntityType = "ItAsset",
                    Timestamp = l.Timestamp.ToString("yyyy-MM-dd HH:mm"),
                    Username = l.Username ?? "Unknown",
                    Details = l.Details ?? ""
                }).ToList();

                // Apparel stats
                dto.TotalApparelTypes = await _context.ApparelList.CountAsync(a => a.CampusId == campusId, cancellationToken);
                dto.TotalApparelInStock = await _context.ApparelItems
                    .Where(i => i.CampusId == campusId && i.Status == "In Stock")
                    .CountAsync(cancellationToken);
                dto.TotalApparelSold = await _context.ApparelItems
                    .Where(i => i.CampusId == campusId && i.Status == "Sold")
                    .CountAsync(cancellationToken);

                dto.ApparelByCategory = await _context.ApparelList
                    .Where(a => a.CampusId == campusId)
                    .GroupBy(a => a.Category)
                    .Select(g => new CategoryCountDto { Category = g.Key ?? "Unknown", Count = g.Count() })
                    .ToListAsync(cancellationToken);

                // Recent activity (Apparel)
                var recentApparelLogsRaw = await _context.AuditLogs
                    .Where(l => l.CampusId == campusId && l.EntityType == "Apparel")
                    .OrderByDescending(l => l.Timestamp)
                    .Take(5)
                    .ToListAsync(cancellationToken);

                dto.RecentApparelActivities = recentApparelLogsRaw.Select(l => new RecentActivityDto
                {
                    Action = l.Action,
                    EntityName = l.EntityName,
                    EntityType = "Apparel",
                    Timestamp = l.Timestamp.ToString("yyyy-MM-dd HH:mm"),
                    Username = l.Username ?? "Unknown",
                    Details = l.Details ?? ""
                }).ToList();

                // Furniture stats
                dto.TotalFurniture = await _context.Furnitures.CountAsync(f => f.CampusId == campusId, cancellationToken);
                dto.FurnitureByLocation = await _context.Furnitures
                    .Where(f => f.CampusId == campusId)
                    .GroupBy(f => f.Location)
                    .Select(g => new LocationCountDto { Location = g.Key ?? "Unknown", Count = g.Count() })
                    .ToListAsync(cancellationToken);
                
                dto.FurnitureByCondition = await _context.Furnitures
                    .Where(f => f.CampusId == campusId)
                    .GroupBy(f => f.Condition)
                    .Select(g => new ConditionCountDto { Condition = g.Key ?? "Good", Count = g.Count() })
                    .ToListAsync(cancellationToken);

                // Recent activity (Furniture)
                var recentFurnitureLogsRaw = await _context.AuditLogs
                    .Where(l => l.CampusId == campusId && l.EntityType == "Furniture")
                    .OrderByDescending(l => l.Timestamp)
                    .Take(5)
                    .ToListAsync(cancellationToken);

                dto.RecentFurnitureActivities = recentFurnitureLogsRaw.Select(l => new RecentActivityDto
                {
                    Action = l.Action,
                    EntityName = l.EntityName,
                    EntityType = "Furniture",
                    Timestamp = l.Timestamp.ToString("yyyy-MM-dd HH:mm"),
                    Username = l.Username ?? "Unknown",
                    Details = l.Details ?? ""
                }).ToList();

                // Combined recent activity
                var recentLogsRaw = await _context.AuditLogs
                    .Where(l => l.CampusId == campusId)
                    .OrderByDescending(l => l.Timestamp)
                    .Take(5)
                    .ToListAsync(cancellationToken);

                dto.RecentActivities = recentLogsRaw.Select(l => new RecentActivityDto
                {
                    Action = l.Action,
                    EntityName = l.EntityName,
                    EntityType = l.EntityType ?? "System",
                    Timestamp = l.Timestamp.ToString("yyyy-MM-dd HH:mm"),
                    Username = l.Username ?? "Unknown",
                    Details = l.Details ?? ""
                }).ToList();

                // Consumables
                dto.TotalConsumableTypes = await _context.ConsumableList.CountAsync(c => c.CampusId == campusId, cancellationToken);
                
                var consumableStats = await _context.ConsumableList
                    .Where(c => c.CampusId == campusId)
                    .Select(c => new { c.In, c.Out })
                    .ToListAsync(cancellationToken);
                dto.TotalConsumableBalance = consumableStats.Sum(c => c.In - c.Out);

                var recentConsumableLogsRaw = await _context.AuditLogs
                    .Where(l => l.CampusId == campusId && l.EntityType == "Consumable")
                    .OrderByDescending(l => l.Timestamp)
                    .Take(5)
                    .ToListAsync(cancellationToken);

                dto.RecentConsumableActivities = recentConsumableLogsRaw.Select(l => new RecentActivityDto
                {
                    Action = l.Action,
                    EntityName = l.EntityName,
                    EntityType = "Consumable",
                    Timestamp = l.Timestamp.ToString("yyyy-MM-dd HH:mm"),
                    Username = l.Username ?? "Unknown",
                    Details = l.Details ?? ""
                }).ToList();
            }
            catch (System.Exception)
            {
                // In case of any individual query failure, we still return the partially populated DTO
                // rather than crashing the whole dashboard.
            }

            return dto;
        }
    }
}
