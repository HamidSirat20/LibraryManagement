using LibraryManagement.WebAPI.Models.Dtos;
using LibraryManagement.WebAPI.Services.Interfaces;
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
        _logger = logger;
        _bookService = bookService;
    }
    [HttpGet]
    public async Task<ActionResult<IEnumerable<BookReadDto>>> GetAllBooks()
    {
        var books = await _bookService.ListAllAsync();
        return Ok(books);

    }
}
