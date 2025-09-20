using LibraryManagement.WebAPI.Models.Dtos;
using LibraryManagement.WebAPI.Services.Interfaces;
using LibraryManagement.WebAPI.Services.ORM;
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
    public async Task<ActionResult<IEnumerable<BookWithPublisherDto>>> ListAllBooks()
    {
        var books = await _bookService.ListAllBooksAsync();

        var bookWithPublisherDto = new List<BookWithPublisherDto>();
        foreach (var book in books)
        {
            bookWithPublisherDto.Add(book.MapBookToBookWithPublisherDto());
        }
        return Ok(bookWithPublisherDto.OrderBy(b =>b.Title));

    }

    [HttpGet("{id}", Name = "GetBookById")]
    public async Task<IActionResult> GetBookById(Guid id, bool includePublisher =false)
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
}
