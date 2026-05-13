using MediatR;
using Microsoft.EntityFrameworkCore;
using QuickStock.Applications.Library.Command;
using QuickStock.Infrastructure.Data;
using QuickStock.Domain.Library;
using QuickStock.Domain.Shared;
using System.Security.Claims;

namespace QuickStock.Applications.Library.Handler
{
    public class LibraryItemHandlers :
        IRequestHandler<AddLibraryBookItemCommand, LibraryBookItem>,
        IRequestHandler<UpdateLibraryBookItemCommand, bool>,
        IRequestHandler<DeleteLibraryBookItemCommand, bool>
    {
        private readonly AppDbContext _context;
        private readonly QuickStock.Infrastructure.Services.INotificationService _notificationService;

        public LibraryItemHandlers(AppDbContext context, QuickStock.Infrastructure.Services.INotificationService notificationService)
        {
            _context = context;
            _notificationService = notificationService;
        }

        public async Task<LibraryBookItem> Handle(AddLibraryBookItemCommand request, CancellationToken cancellationToken)
        {
            if (request.User.IsInRole("Viewer"))
                throw new UnauthorizedAccessException("Viewers cannot add book items.");

            var item = request.Item;
            item.LibrarydataId = request.BookId;
            item.CreatedAt = DateTime.UtcNow;
            item.UpdatedAt = DateTime.UtcNow;

            if (string.IsNullOrWhiteSpace(item.QRCode))
            {
                item.QRCode = "LIB-ITEM-" + Guid.NewGuid().ToString("N").Substring(0, 10).ToUpper();
            }

            _context.LibraryBookItems.Add(item);
            await _context.SaveChangesAsync(cancellationToken);

            await LogAction(request.User, "Add Item", item.Id, item.AccessionNumber, $"Added copy {item.AccessionNumber} to book ID {request.BookId}", item.CampusId, item.Status);

            await _notificationService.NotifyCampusActivity(item.CampusId, "New Book Copy Added", 
                $"{request.User.Identity?.Name} added copy {item.AccessionNumber}", "Success");

            return item;
        }

        public async Task<bool> Handle(UpdateLibraryBookItemCommand request, CancellationToken cancellationToken)
        {
            if (request.User.IsInRole("Viewer"))
                throw new UnauthorizedAccessException("Viewers cannot update book items.");

            var item = await _context.LibraryBookItems.FindAsync(new object[] { request.ItemId }, cancellationToken);
            if (item == null) return false;

            item.AccessionNumber = request.Item.AccessionNumber;
            item.Condition = request.Item.Condition;
            item.Status = request.Item.Status;
            item.UpdatedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync(cancellationToken);
            await LogAction(request.User, "Update Item", item.Id, item.AccessionNumber, $"Updated copy {item.AccessionNumber}", item.CampusId, item.Status);

            await _notificationService.NotifyCampusActivity(item.CampusId, "Book Copy Updated", 
                $"{request.User.Identity?.Name} updated copy {item.AccessionNumber}", "Info");

            return true;
        }

        public async Task<bool> Handle(DeleteLibraryBookItemCommand request, CancellationToken cancellationToken)
        {
            if (request.User.IsInRole("Viewer"))
                throw new UnauthorizedAccessException("Viewers cannot delete book items.");

            var item = await _context.LibraryBookItems.FindAsync(new object[] { request.ItemId }, cancellationToken);
            if (item == null) return false;

            var campusId = item.CampusId;
            var accession = item.AccessionNumber;

            _context.LibraryBookItems.Remove(item);
            await _context.SaveChangesAsync(cancellationToken);
            await LogAction(request.User, "Delete Item", item.Id, accession, $"Deleted copy {accession}", campusId, item.Status);

            await _notificationService.NotifyCampusActivity(campusId, "Book Copy Removed", 
                $"{request.User.Identity?.Name} deleted copy {accession}", "Warning");

            return true;
        }

        private async Task LogAction(ClaimsPrincipal user, string action, int entityId, string entityName, string details, int campusId, string? status)
        {
            var userId = user.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            var username = user.Identity?.Name;

            var log = new AuditLog
            {
                Action = action,
                EntityType = "LibraryItem",
                EntityId = entityId,
                EntityName = entityName,
                Details = details,
                UserId = userId,
                Username = username,
                CampusId = campusId,
                Status = status,
                Timestamp = DateTime.UtcNow
            };
            _context.AuditLogs.Add(log);
            await _context.SaveChangesAsync();
        }
    }
}
