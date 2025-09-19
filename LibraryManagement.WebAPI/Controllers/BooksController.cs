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
    public async Task<ActionResult<BookWithPublisherDto>> GetBookById(Guid id, bool includePublisher =false)
    {
        var book = await _bookService.GetByIdAsync(id, includePublisher);
        if (book is null) 
        {
            return NotFound();
        }

        if(includePublisher)
        {
            var bookDto = book.MapBookToBookWithPublisherDto();
            return Ok(bookDto);
        }
        var bookWithPublisherDto = book.MapBookToBookWithoutPublisherDto();
        return Ok(bookWithPublisherDto);
    }
}
