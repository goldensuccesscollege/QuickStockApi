using Microsoft.EntityFrameworkCore;
using QuickStock.Applications.Dashboard.Dto_s;
using QuickStock.Applications.Dashboard.Queries;
using QuickStock.CQRS;
using QuickStock.Domain.Shared;
using QuickStock.Infrastructure.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace QuickStock.Applications.Dashboard.Handler
{
    public class GetDashboardStatsHandler : IRequestHandler<GetDashboardStatsQuery, DashboardDto>
    {
        private readonly AppDbContext _context;

        public GetDashboardStatsHandler(AppDbContext context)
        {
            _context = context;
        }

        public async Task<DashboardDto> Handle(
            GetDashboardStatsQuery request,
            CancellationToken cancellationToken)
        {
            var campusId = request.CampusId;
            var dto = new DashboardDto();

            try
            {
                var currentMonth = DateTime.UtcNow.Month;
                var currentYear = DateTime.UtcNow.Year;
                var startOfMonth = new DateTime(currentYear, currentMonth, 1, 0, 0, 0, DateTimeKind.Utc);
                var startOfNextMonth = startOfMonth.AddMonths(1);

                // 1. Verify Campus exists and set baseline
                var campusExists = await _context.Campuses.AnyAsync(c => c.CampusId == campusId, cancellationToken);
                if (!campusExists)
                {
                    dto.SystemOverview = "No active campus matching query specifications found.";
                    return dto;
                }

                // 2. Fetch IT Assets Statistics
                try
                {
                    dto.TotalAssets = await _context.Itassets.CountAsync(a => a.CampusId == campusId, cancellationToken);
                    dto.ActiveAssets = await _context.Itassets.CountAsync(a => a.CampusId == campusId && a.Status == "Active", cancellationToken);
                    dto.CriticalAssets = await _context.Itassets.CountAsync(a => a.CampusId == campusId && (a.Status == "Damaged" || a.Status == "For Repair"), cancellationToken);
                    dto.NewAssetsThisMonth = await _context.Itassets.CountAsync(a => a.CampusId == campusId && a.DateAdded >= startOfMonth && a.DateAdded < startOfNextMonth, cancellationToken);
                }
                catch (Exception ex)
                {
                    dto.Alerts.Add($"IT Assets statistics fetch failed: {ex.Message}");
                }

                // 3. Fetch Rooms Statistics
                try
                {
                    dto.RoomCount = await _context.Rooms.CountAsync(r => r.CampusId == campusId, cancellationToken);
                    dto.DisabledRooms = await _context.Rooms.CountAsync(r => r.CampusId == campusId && r.IsDisabled, cancellationToken);
                }
                catch (Exception ex)
                {
                    dto.Alerts.Add($"Rooms statistics fetch failed: {ex.Message}");
                }

                // 4. Fetch Apparel Statistics
                try
                {
                    dto.TotalApparelTypes = await _context.ApparelList.CountAsync(a => a.CampusId == campusId, cancellationToken);
                    dto.TotalApparelInStock = await _context.ApparelItems.CountAsync(a => a.CampusId == campusId && a.Status == "In Stock", cancellationToken);
                    dto.TotalApparelSold = await _context.ApparelItems.CountAsync(a => a.CampusId == campusId && a.Status == "Sold", cancellationToken);
                }
                catch (Exception ex)
                {
                    dto.Alerts.Add($"Apparel statistics fetch failed: {ex.Message}");
                }

                // 5. Fetch Furniture Statistics
                try
                {
                    dto.TotalFurniture = await _context.Furnitures.CountAsync(f => f.CampusId == campusId, cancellationToken);
                }
                catch (Exception ex)
                {
                    dto.Alerts.Add($"Furniture statistics fetch failed: {ex.Message}");
                }

                // 6. Fetch Consumable Statistics
                try
                {
                    dto.TotalConsumableTypes = await _context.ConsumableUnits.CountAsync(c => c.CampusId == campusId, cancellationToken);
                    
                    var countQuery = _context.ConsumableUnits.Where(c => c.CampusId == campusId && c.Count != null);
                    dto.TotalConsumableBalance = await countQuery.AnyAsync(cancellationToken)
                        ? await countQuery.SumAsync(c => c.Count!.Value, cancellationToken)
                        : 0;
                }
                catch (Exception ex)
                {
                    dto.Alerts.Add($"Consumables statistics fetch failed: {ex.Message}");
                }

                // 7. Fetch Monthly Activities Count
                try
                {
                    dto.MonthlyActivities = await _context.AuditLogs.CountAsync(l => l.CampusId == campusId && l.Timestamp >= startOfMonth && l.Timestamp < startOfNextMonth, cancellationToken);
                }
                catch (Exception ex)
                {
                    dto.Alerts.Add($"Audit logs count fetch failed: {ex.Message}");
                }

                // Health percentage
                dto.AssetHealthPercentage = dto.TotalAssets == 0
                    ? 100 
                    : (int)(((double)dto.ActiveAssets / dto.TotalAssets) * 100);

                var assetsQuery = _context.Itassets.Where(a => a.CampusId == campusId);

                // Status Distribution
                try
                {
                    dto.AssetsByStatus = await assetsQuery
                        .GroupBy(a => a.Status)
                        .Select(g => new StatusCountDto
                        {
                            Status = g.Key ?? "Unknown",
                            Count = g.Count()
                        })
                        .OrderByDescending(x => x.Count)
                        .ToListAsync(cancellationToken);
                }
                catch (Exception ex)
                {
                    dto.Alerts.Add($"Assets by Status query failed: {ex.Message}");
                }

                // Type Distribution
                try
                {
                    dto.AssetsByType = await assetsQuery
                        .GroupBy(a => a.Type)
                        .Select(g => new TypeCountDto
                        {
                            Type = g.Key ?? "Unknown",
                            Count = g.Count()
                        })
                        .OrderByDescending(x => x.Count)
                        .ToListAsync(cancellationToken);
                    
                    dto.MostCommonAssetType = dto.AssetsByType.FirstOrDefault()?.Type ?? "None";
                }
                catch (Exception ex)
                {
                    dto.Alerts.Add($"Assets by Type query failed: {ex.Message}");
                    dto.MostCommonAssetType = "None";
                }

                // Room Distribution
                try
                {
                    dto.AssetsByRoom = await assetsQuery
                        .GroupBy(a => a.Room != null ? a.Room.RoomName : "Unassigned")
                        .Select(g => new RoomAssetDto
                        {
                            Room = g.Key,
                            Count = g.Count()
                        })
                        .OrderByDescending(x => x.Count)
                        .ToListAsync(cancellationToken);
                }
                catch (Exception ex)
                {
                    dto.Alerts.Add($"Assets by Room query failed: {ex.Message}");
                }

                // Most Problematic Room
                try
                {
                    dto.MostProblematicRoom = await assetsQuery
                        .Where(a => a.Status == "Damaged" || a.Status == "For Repair")
                        .GroupBy(a => a.Room != null ? a.Room.RoomName : "Unassigned")
                        .OrderByDescending(g => g.Count())
                        .Select(g => g.Key)
                        .FirstOrDefaultAsync(cancellationToken) ?? "None";
                }
                catch (Exception ex)
                {
                    dto.Alerts.Add($"Most Problematic Room query failed: {ex.Message}");
                    dto.MostProblematicRoom = "None";
                }

                // Apparel By Category
                try
                {
                    dto.ApparelByCategory = await _context.ApparelList
                        .Where(a => a.CampusId == campusId)
                        .GroupBy(a => a.Category)
                        .Select(g => new CategoryCountDto
                        {
                            Category = g.Key ?? "Unknown",
                            Count = g.Count()
                        })
                        .OrderByDescending(x => x.Count)
                        .ToListAsync(cancellationToken);
                }
                catch (Exception ex)
                {
                    dto.Alerts.Add($"Apparel by Category query failed: {ex.Message}");
                }

                // Furniture By Location
                try
                {
                    dto.FurnitureByLocation = await _context.Furnitures
                        .Where(f => f.CampusId == campusId)
                        .GroupBy(f => f.Location)
                        .Select(g => new LocationCountDto
                        {
                            Location = g.Key ?? "Unknown",
                            Count = g.Count()
                        })
                        .OrderByDescending(x => x.Count)
                        .ToListAsync(cancellationToken);
                }
                catch (Exception ex)
                {
                    dto.Alerts.Add($"Furniture by Location query failed: {ex.Message}");
                }

                // Furniture By Condition
                try
                {
                    dto.FurnitureByCondition = await _context.Furnitures
                        .Where(f => f.CampusId == campusId)
                        .GroupBy(f => f.Condition)
                        .Select(g => new ConditionCountDto
                        {
                            Condition = g.Key ?? "Good",
                            Count = g.Count()
                        })
                        .OrderByDescending(x => x.Count)
                        .ToListAsync(cancellationToken);
                }
                catch (Exception ex)
                {
                    dto.Alerts.Add($"Furniture by Condition query failed: {ex.Message}");
                }

                // Audit Logs / Activity Streams
                try
                {
                    var recentLogsRaw = await _context.AuditLogs
                        .Where(l => l.CampusId == campusId)
                        .OrderByDescending(l => l.Timestamp)
                        .Take(50)
                        .ToListAsync(cancellationToken);

                    var userIds = recentLogsRaw
                        .Where(l => !string.IsNullOrEmpty(l.UserId))
                        .Select(l => l.UserId!)
                        .Distinct()
                        .ToList();

                    var accountIds = userIds
                        .Where(id => int.TryParse(id, out _))
                        .Select(int.Parse)
                        .ToList();

                    var accounts = await _context.Accounts
                        .Include(a => a.Profile)
                        .Where(a => accountIds.Contains(a.Id))
                        .ToListAsync(cancellationToken);

                    string ResolveUsername(string? rawUsername, string? userId)
                    {
                        var username = rawUsername ?? "Unknown";
                        if (!string.IsNullOrEmpty(userId) && int.TryParse(userId, out int accountId))
                        {
                            var acc = accounts.FirstOrDefault(a => a.Id == accountId);
                            if (acc?.Profile != null)
                            {
                                username = $"{acc.Profile.FirstName} {acc.Profile.LastName}".Trim();
                            }
                        }
                        return username;
                    }

                    dto.RecentActivities = recentLogsRaw
                        .Select(l => new RecentActivityDto
                        {
                            Action = l.Action,
                            EntityName = l.EntityName,
                            EntityType = l.EntityType ?? "System",
                            Timestamp = l.Timestamp.ToString("yyyy-MM-dd HH:mm"),
                            Username = ResolveUsername(l.Username, l.UserId),
                            Details = l.Details ?? ""
                        })
                        .ToList();

                    dto.RecentItActivities = dto.RecentActivities
                        .Where(a => a.EntityType.ToLower().Contains("it") || a.EntityType.ToLower() == "asset" || a.EntityType.ToLower() == "room")
                        .Take(5)
                        .ToList();

                    dto.RecentApparelActivities = dto.RecentActivities
                        .Where(a => a.EntityType.ToLower().Contains("apparel"))
                        .Take(5)
                        .ToList();

                    dto.RecentFurnitureActivities = dto.RecentActivities
                        .Where(a => a.EntityType.ToLower().Contains("furniture"))
                        .Take(5)
                        .ToList();

                    dto.RecentConsumableActivities = dto.RecentActivities
                        .Where(a => a.EntityType.ToLower().Contains("consumable"))
                        .Take(5)
                        .ToList();
                }
                catch (Exception ex)
                {
                    dto.Alerts.Add($"Recent activities generation failed: {ex.Message}");
                }

                if (dto.DisabledRooms > 0)
                    dto.Alerts.Add($"{dto.DisabledRooms} rooms are currently disabled and out of rotational access.");

                if (dto.CriticalAssets > 0)
                    dto.Alerts.Add($"{dto.CriticalAssets} IT assets require prioritized hardware maintenance.");

                if (dto.AssetHealthPercentage < 70)
                    dto.Alerts.Add($"Asset baseline operational availability is low ({dto.AssetHealthPercentage}%).");

                dto.SystemOverview = $"Campus currently maintains {dto.TotalAssets} total IT assets, " +
                                     $"{dto.ActiveAssets} active deployments, {dto.CriticalAssets} fault items, and " +
                                     $"{dto.DisabledRooms} closed operational zones.";
            }
            catch (Exception ex)
            {
                dto.SystemOverview = "Dashboard loaded with partial data due to an internal execution fault.";
                dto.Alerts.Add(ex.Message);
            }

            return dto;
        }
    }
}
