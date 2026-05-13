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
using System.Security.Claims;
using System.Threading;
using System.Threading.Tasks;
using QuickStock.Common.Exceptions;

namespace QuickStock.Applications.Apparel.Handler
{
    public class DeleteApparelHandler : IRequestHandler<DeleteApparelCommand, bool>
    {
        private readonly AppDbContext _context;

        public DeleteApparelHandler(AppDbContext context)
        {
            _context = context;
        }

        public async Task<bool> Handle(DeleteApparelCommand request, CancellationToken cancellationToken)
        {
            var apparel = await _context.ApparelList.FindAsync(new object[] { request.Id }, cancellationToken);
            if (apparel == null) return false;

            // Check if there is existing apparel Quantity
            if (apparel.Quality_In_Stock > 0)
            {
                throw new BadRequestException("Cannot delete apparel because there are still items in stock.");
            }

            var apparelName = apparel.Apparel_Name;
            var campusId = apparel.CampusId;

            _context.ApparelList.Remove(apparel);
            await _context.SaveChangesAsync(cancellationToken);

            await LogAction(request.User, "Delete", request.Id, apparelName, $"Deleted apparel: {apparelName}", campusId, null);

            return true;
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
