using LibraryManagement.WebAPI.Models.Common;
using LibraryManagement.WebAPI.Models.Dtos;
using LibraryManagement.WebAPI.Services.Interfaces;
using LibraryManagement.WebAPI.Services.ORM;
using Microsoft.AspNetCore.JsonPatch;
using Microsoft.AspNetCore.Mvc;

namespace LibraryManagement.WebAPI.Controllers;

[ApiController]
[Route("api/v1/[controller]")]
public class BooksController : ControllerBase
{
    private readonly ILogger<BooksController> _logger;
    private readonly IBookService _bookService;

    public BooksController(ILogger<BooksController> logger, IBookService bookService)
    {
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _bookService = bookService ?? throw new ArgumentNullException(nameof(bookService));
    }
    [HttpGet]
    public async Task<ActionResult<IEnumerable<BookWithPublisherDto>>> ListAllBooks([FromQuery] QueryOptions queryOptions)
    {
        var books = await _bookService.ListAllBooksAsync(queryOptions);

        var bookWithPublisherDto = new List<BookWithPublisherDto>();
        foreach (var book in books)
        {
            bookWithPublisherDto.Add(book.MapBookToBookWithPublisherDto());
        }
        return Ok(bookWithPublisherDto);

    }

    [HttpGet("{id}", Name = "GetBookById")]
    public async Task<IActionResult> GetBookById(Guid id, bool includePublisher = false)
    {
        var book = await _bookService.GetByIdAsync(id, includePublisher);
        if (book is null)
        {
            _logger.LogDebug("Book with id: {id} was not found", id);
            return NotFound();
        }

        if (includePublisher)
        {
            var bookDto = book.MapBookToBookWithPublisherDto();
            return Ok(bookDto);
        }
        var bookWithPublisherDto = book.MapBookToBookWithoutPublisherDto();
        return Ok(bookWithPublisherDto);
    }
    [HttpPost]
    public async Task<IActionResult> CreateBook([FromBody] BookCreateDto bookCreateDto)
    {
        if (bookCreateDto == null)
        {
            _logger.LogDebug("BookCreateDto object sent from client is null.");
            return BadRequest("BookCreateDto object is null");
        }
        if (!ModelState.IsValid)
        {
            _logger.LogDebug("Invalid model state for the BookCreateDto object");
            return UnprocessableEntity(ModelState);
        }
        var createdBook = await _bookService.CreateBookAsync(bookCreateDto);
        var bookReadDto = createdBook.MapBookToBookReadDto();
        return CreatedAtAction("GetBookById", new { id = bookReadDto.Id }, bookReadDto);
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult>  DeleteBookById (Guid id)
    {
        if(! await _bookService.EntityExistAsync(id))
        {
            _logger.LogDebug("Book with id: {id} was not found", id);
            return NotFound();
        }
        var result = await _bookService.DeleteBookByIdAsync(id);
        if(result == null || result == false)
        {
            _logger.LogError("Something went wrong while deleting the book with id: {id}", id);
            return StatusCode(500, "A problem happened while handling your request.");
        }
        return NoContent();
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> UpdateBook(Guid id, [FromBody] BookUpdateDto bookUpdateDto)
    {
        if (bookUpdateDto == null)
        {
            return BadRequest("BookUpdateDto object is null");
        }

        if (!ModelState.IsValid)
        {
            return UnprocessableEntity(ModelState);
        }

        try
        {
            var updatedBook = await _bookService.UpdateBookAsync(id, bookUpdateDto);
            return NoContent(); 
        }
        catch (ArgumentException)
        {
            return NotFound();
        }
    }


    [HttpPatch("{id}")]
    public async Task<IActionResult> PartiallyUpdateBookAsync(Guid id, [FromBody] JsonPatchDocument<BookUpdateDto> patchDocument)
    {
        var existingBook = await _bookService.GetByIdAsync(id);

        if (existingBook is null)
        {
            _logger.LogDebug("Book with id: {id} was not found", id);
            return NotFound();
        }

        var bookToPatchDto = existingBook.MapBookToBookUpdateDto();
        patchDocument.ApplyTo(bookToPatchDto, ModelState);

        if (!ModelState.IsValid)
        {
            _logger.LogDebug("Invalid model state for the BookUpdateDto object");
            return BadRequest(ModelState);
        }
        if (!TryValidateModel(bookToPatchDto))
        {
            _logger.LogDebug("Invalid model state for the BookUpdateDto object");
            return UnprocessableEntity(ModelState);
        }

      var bookForUpdate =  bookToPatchDto.MapBookUpdateDtoToBook(existingBook);
        await _bookService.PartiallyUpdateBookAsync(bookForUpdate);
        return NoContent();
    }

}
