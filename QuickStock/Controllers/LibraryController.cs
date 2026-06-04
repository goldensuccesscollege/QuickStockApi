using QuickStock.CQRS;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using QuickStock.Applications.Library.Command;
using QuickStock.Applications.Library.Queries;
using QuickStock.Domain.Library;

namespace QuickStock.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class LibraryController : ControllerBase
    {
        private readonly IMediator _mediator;

        public LibraryController(IMediator mediator) => _mediator = mediator;

        [HttpGet]
        public async Task<ActionResult<IEnumerable<Librarydata>>> GetBooks(int? campusId = null)
        {
            var result = await _mediator.Send(new GetLibraryBooksQuery(campusId, User));
            return Ok(result);
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<Librarydata>> GetBook(int id)
        {
            var result = await _mediator.Send(new GetLibraryBookByIdQuery(id, User));
            if (result == null) return NotFound();
            return Ok(result);
        }

        [HttpPost]
        [Authorize(Roles = "Admin,Manager,Employee")]
        public async Task<ActionResult<Librarydata>> Create(Librarydata book)
        {
            var result = await _mediator.Send(new CreateLibraryBookCommand(book, User));
            return CreatedAtAction(nameof(GetBook), new { id = result.ItemId }, result);
        }

        [HttpPut("{id}")]
        [Authorize(Roles = "Admin,Manager")]
        public async Task<IActionResult> Update(int id, Librarydata book)
        {
            if (id != book.ItemId) return BadRequest();
            var success = await _mediator.Send(new UpdateLibraryBookCommand(id, book, User));
            if (!success) return NotFound();
            return NoContent();
        }

        // --- Item Management ---

        [HttpPost("{bookId}/items")]
        [Authorize(Roles = "Admin,Manager,Employee")]
        public async Task<ActionResult<LibraryBookItem>> AddItem(int bookId, LibraryBookItem item)
        {
            var result = await _mediator.Send(new AddLibraryBookItemCommand(bookId, item, User));
            return Ok(result);
        }

        [HttpPut("items/{itemId}")]
        [Authorize(Roles = "Admin,Manager")]
        public async Task<IActionResult> UpdateItem(int itemId, LibraryBookItem item)
        {
            var success = await _mediator.Send(new UpdateLibraryBookItemCommand(itemId, item, User));
            if (!success) return NotFound();
            return NoContent();
        }

        [HttpDelete("items/{itemId}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> DeleteItem(int itemId)
        {
            var success = await _mediator.Send(new DeleteLibraryBookItemCommand(itemId, User));
            if (!success) return NotFound();
            return NoContent();
        }
    }
}
