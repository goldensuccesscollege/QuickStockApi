using QuickStock.CQRS;
using Microsoft.EntityFrameworkCore;
using QuickStock.Applications.Apparel.Command;
using QuickStock.Infrastructure.Data;
using QuickStock.Domain.Apparel;
using QuickStock.Domain.ITassets;
using QuickStock.Domain.Locations;
using QuickStock.Domain.Shared;
using QuickStock.Domain.Accounts;
using System.Security.Claims;
using System.Threading;
using System.Threading.Tasks;

namespace QuickStock.Applications.Apparel.Handler
{
    public class UpdateApparelHandler : IRequestHandler<UpdateApparelCommand, bool>
    {
        private readonly AppDbContext _context;

        public UpdateApparelHandler(AppDbContext context)
        {
            _context = context;
        }

        public async Task<bool> Handle(UpdateApparelCommand request, CancellationToken cancellationToken)
        {
            var existingApparel = await _context.ApparelList.FindAsync(new object[] { request.Id }, cancellationToken);
            if (existingApparel == null) return false;

            existingApparel.Apparel_Name = request.Apparel.Apparel_Name;
            existingApparel.Category = request.Apparel.Category;
            existingApparel.Sex = request.Apparel.Sex;
            existingApparel.Size = request.Apparel.Size;
            existingApparel.Grade_Level = request.Apparel.Grade_Level;
            existingApparel.Quality_In_Stock = request.Apparel.Quality_In_Stock;
            existingApparel.Reorder_level = request.Apparel.Reorder_level;
            existingApparel.Date_Purchased = request.Apparel.Date_Purchased;
            existingApparel.Unit_Price = request.Apparel.Unit_Price;
            existingApparel.Supplier_Name = request.Apparel.Supplier_Name;
            existingApparel.Remarks = request.Apparel.Remarks;
            existingApparel.Location = request.Apparel.Location;

            await _context.SaveChangesAsync(cancellationToken);
            await LogAction(request.User, "Update", request.Apparel.Apparel_ID, request.Apparel.Apparel_Name, $"Updated apparel details", request.Apparel.CampusId, null);

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
