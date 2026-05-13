using MediatR;
using Microsoft.EntityFrameworkCore;
using QuickStock.Applications.Apparel.Command;
using QuickStock.Infrastructure.Data;
using QuickStock.Domain.Apparel;
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
using System;
using System.Collections.Generic;
using System.Security.Claims;
using System.Threading;
using System.Threading.Tasks;

namespace QuickStock.Applications.Apparel.Handler
{
    public class CreateApparelHandler : IRequestHandler<CreateApparelCommand, Appareldata>
    {
        private readonly AppDbContext _context;
        private readonly QuickStock.Infrastructure.Services.INotificationService _notificationService;

        public CreateApparelHandler(AppDbContext context, QuickStock.Infrastructure.Services.INotificationService notificationService)
        {
            _context = context;
            _notificationService = notificationService;
        }

        public async Task<Appareldata> Handle(CreateApparelCommand request, CancellationToken cancellationToken)
        {
            var apparel = request.Apparel;
            using var transaction = await _context.Database.BeginTransactionAsync(cancellationToken);
            try
            {
                _context.ApparelList.Add(apparel);
                await _context.SaveChangesAsync(cancellationToken);

                // Automatically generate individual items based on quantity
                var nextNum = 1;
                var items = new List<ApparelItem>();
                for (int i = 1; i <= apparel.Quality_In_Stock; i++)
                {
                    items.Add(new ApparelItem
                    {
                        ApparelType = apparel,
                        Apparel_Number = $"{apparel.Apparel_Name.Substring(0, 3).ToUpper()}-{nextNum++:D4}",
                        Status = "In Stock",
                        CampusId = apparel.CampusId,
                        DateCreated = DateTime.UtcNow,
                        LastModified = DateTime.UtcNow
                    });
                }
                _context.ApparelItems.AddRange(items);
                await _context.SaveChangesAsync(cancellationToken);

                await transaction.CommitAsync(cancellationToken);

                await LogAction(request.User, "Register", apparel.Apparel_ID, apparel.Apparel_Name, 
                    $"Registered new apparel type: {apparel.Apparel_Name} ({apparel.Category})", apparel.CampusId, "In Stock");

                await _notificationService.NotifyCampusActivity(apparel.CampusId, "New Apparel Registered", 
                    $"{request.User.Identity?.Name} registered {apparel.Apparel_Name}", "Success");
                return apparel;
            }
            catch (Exception)
            {
                await transaction.RollbackAsync(cancellationToken);
                throw;
            }
        }

        private async Task LogAction(ClaimsPrincipal user, string action, int entityId, string entityName, string details, int campusId, string? status)
        {
            var userId = user.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            var username = user.Identity?.Name;
            
            var log = new AuditLog
            {
                Action = action,
                EntityType = "Apparel",
                EntityId = entityId,
                EntityName = entityName,
                Details = details,
                UserId = userId,
                Username = username,
                CampusId = campusId,
                Status = status
            };
            _context.AuditLogs.Add(log);
            await _context.SaveChangesAsync();
        }
    }
}
