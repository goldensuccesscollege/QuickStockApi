using QuickStock.CQRS;
using Microsoft.EntityFrameworkCore;
using QuickStock.Applications.Library.Command;
using QuickStock.Applications.Library.Queries;
using QuickStock.Infrastructure.Data;
using QuickStock.Domain.Library;
using QuickStock.Domain.Shared;
using QuickStock.Domain.Locations;
using System.Security.Claims;

namespace QuickStock.Applications.Library.Handler
{
    public class LibraryHandlers : 
        IRequestHandler<CreateLibraryBookCommand, Librarydata>,
        IRequestHandler<UpdateLibraryBookCommand, bool>,
        IRequestHandler<GetLibraryBooksQuery, IEnumerable<Librarydata>>,
        IRequestHandler<GetLibraryBookByIdQuery, Librarydata?>
    {
        private readonly AppDbContext _context;
        private readonly QuickStock.Infrastructure.Services.INotificationService _notificationService;

        public LibraryHandlers(AppDbContext context, QuickStock.Infrastructure.Services.INotificationService notificationService)
        {
            _context = context;
            _notificationService = notificationService;
        }

        public async Task<Librarydata> Handle(CreateLibraryBookCommand request, CancellationToken cancellationToken)
        {
            if (request.User.IsInRole("Viewer"))
                throw new UnauthorizedAccessException("Viewers cannot create books.");

            var book = request.Book;
            
            book.CreatedAt = DateTime.UtcNow;
            book.UpdatedAt = DateTime.UtcNow;

            _context.LibraryBooks.Add(book);
            await _context.SaveChangesAsync(cancellationToken);

            // Create initial item if accession number provided
            var accession = book.InitialAccessionNumber; // Capture it before EF might do anything
            if (!string.IsNullOrWhiteSpace(accession))
            {
                var initialItem = new LibraryBookItem
                {
                    LibrarydataId = book.ItemId,
                    AccessionNumber = accession,
                    Status = "Available",
                    DateAcquired = DateTime.UtcNow,
                    CampusId = book.CampusId,
                    QRCode = $"LIB-{book.InitialAccessionNumber}"
                };
                _context.LibraryBookItems.Add(initialItem);
                await _context.SaveChangesAsync(cancellationToken);
            }

            await LogAction(request.User, "Create", book.ItemId, book.Title, $"Added new book: {book.Title}", book.CampusId, null);
            
            await _notificationService.NotifyCampusActivity(book.CampusId, "New Library Book", 
                $"{request.User.Identity?.Name} added {book.Title}", "Success");

            return book;
        }

        public async Task<bool> Handle(UpdateLibraryBookCommand request, CancellationToken cancellationToken)
        {
            if (request.User.IsInRole("Viewer"))
                throw new UnauthorizedAccessException("Viewers cannot update books.");

            var book = await _context.LibraryBooks.FindAsync(new object[] { request.Id }, cancellationToken);
            if (book == null) return false;

            // Map updates
            book.BookNumber = request.Book.BookNumber;
            book.Title = request.Book.Title;
            book.Subtitle = request.Book.Subtitle;
            book.Author = request.Book.Author;
            book.CoAuthor = request.Book.CoAuthor;
            book.Class = request.Book.Class;
            book.Publisher = request.Book.Publisher;
            book.Year = request.Book.Year;
            book.Edition = request.Book.Edition;
            book.Volumes = request.Book.Volumes;
            book.Pages = request.Book.Pages;
            book.ISBN = request.Book.ISBN;
            book.Genre = request.Book.Genre;
            book.Language = request.Book.Language;
            book.ShelfLocation = request.Book.ShelfLocation;
            book.CallNumber = request.Book.CallNumber;
            book.Remarks = request.Book.Remarks;
            book.CostPrice = request.Book.CostPrice;
            book.SourceOfFund = request.Book.SourceOfFund;
            book.Donor = request.Book.Donor;
            book.AcquisitionType = request.Book.AcquisitionType;
            book.DateReceived = request.Book.DateReceived;
            book.CampusId = request.Book.CampusId;
            book.UpdatedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync(cancellationToken);

            await LogAction(request.User, "Update", book.ItemId, book.Title, $"Updated book: {book.Title}", book.CampusId, null);

            await _notificationService.NotifyCampusActivity(book.CampusId, "Library Book Updated", 
                $"{request.User.Identity?.Name} updated {book.Title}", "Info");

            return true;
        }

        public async Task<IEnumerable<Librarydata>> Handle(GetLibraryBooksQuery request, CancellationToken cancellationToken)
        {
            var query = _context.LibraryBooks.Include(b => b.Items).AsQueryable();
            if (request.CampusId.HasValue)
            {
                query = query.Where(b => b.CampusId == request.CampusId.Value);
            }
            return await query.OrderByDescending(b => b.CreatedAt).ToListAsync(cancellationToken);
        }

        public async Task<Librarydata?> Handle(GetLibraryBookByIdQuery request, CancellationToken cancellationToken)
        {
            return await _context.LibraryBooks
                .Include(b => b.Items)
                .FirstOrDefaultAsync(b => b.ItemId == request.Id, cancellationToken);
        }

        private async Task LogAction(ClaimsPrincipal user, string action, int entityId, string entityName, string details, int campusId, string? status)
        {
            var userId = user.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            var username = user.Identity?.Name;

            var log = new AuditLog
            {
                Action = action,
                EntityType = "Library",
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
