using Asp.Versioning;
using LibraryManagement.WebAPI.Helpers;
using LibraryManagement.WebAPI.Models;
using LibraryManagement.WebAPI.Models.Common;
using LibraryManagement.WebAPI.Models.Dtos;
using LibraryManagement.WebAPI.Services.Interfaces;
using LibraryManagement.WebAPI.Services.ORM;
using LibraryManagement.WebAPI.Services.ORM.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.JsonPatch;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.Extensions.Options;
using System.Text.Json;

namespace LibraryManagement.WebAPI.Controllers;

[ApiController]
//[Authorize]
[Route("api/v{version:ApiVersion}/[controller]")]
[ApiVersion("1.0")]
public class BooksController : ControllerBase
{
    private readonly ILogger<BooksController> _logger;
    private readonly IBookService _bookService;
    private readonly IBookMapper _bookMapper;

    public BooksController(ILogger<BooksController> logger, IBookService bookService, IBookMapper bookMapper)
    {
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _bookService = bookService ?? throw new ArgumentNullException(nameof(bookService));
        _bookMapper = bookMapper;
    }

    [HttpGet(Name ="GetAllBooks")]
    public async Task<ActionResult<PaginatedResponse<BookWithPublisherDto>>> ListAllBooks([FromQuery] QueryOptions queryOptions)
    {
        var books = await _bookService.ListAllBooksAsync(queryOptions);
        //var paginationMetadata
        var previousPageLink = books.HasPrevious ? GenerateBooksResourceUri(queryOptions, ResourceUriType.PreviousPage) : null;
       // next page link
        var nextPageLink = books.HasNext ? GenerateBooksResourceUri(queryOptions, ResourceUriType.NextPage) : null;

        // var paginationMetadata 
        var paginationMetadata = new
        {
            totalCount = books.TotalRecords,
            pageSize = books.PageSize,
            currentPage = books.CurrentPage,
            totalPages = books.TotalPages,
            previousPageLink,
            nextPageLink
        };

        var bookWithPublisherDto = new List<BookWithPublisherDto>();
        foreach (var book in books)
        {
            bookWithPublisherDto.Add(_bookMapper.ToBookWithPublisherDto(book));
        }
        Response.Headers["X-Pagination"] = JsonSerializer.Serialize(paginationMetadata);
        return Ok(bookWithPublisherDto);

    }
    private string? GenerateBooksResourceUri(QueryOptions queryOptions, ResourceUriType type)
    {
        switch (type)
        {
            case ResourceUriType.PreviousPage:
                return Url.Link("GetAllBooks",
                    new
                    {
                        page = queryOptions.PageNumber - 1,
                        size = queryOptions.PageSize,
                        search = queryOptions.SearchTerm,
                        genre = queryOptions.Genre,
                        sort = queryOptions.OrderBy,
                        desc = queryOptions.IsDescending
                    });
            case ResourceUriType.NextPage:
                return Url.Link("GetAllBooks",
                    new
                    {
                        page = queryOptions.PageNumber + 1,
                        size = queryOptions.PageSize,
                        search = queryOptions.SearchTerm,
                        genre = queryOptions.Genre,
                        sort = queryOptions.OrderBy,
                        desc = queryOptions.IsDescending
                    });
            default:
                return Url.Link("GetAllBooks",
                    new
                    {
                        page = queryOptions.PageNumber,
                        size = queryOptions.PageSize,
                        search = queryOptions.SearchTerm,
                        genre = queryOptions.Genre,
                        sort = queryOptions.OrderBy,
                        desc = queryOptions.IsDescending
                    });
        }
    }

    [HttpGet("{id}",Name = "GetBookById")]
    public async Task<IActionResult> GetBookById(Guid id,[FromQuery] bool includePublisher = false)
    {
        var book = await _bookService.GetByIdAsync(id, includePublisher);
        if (book is null)
        {
            _logger.LogDebug("Book with id: {id} was not found", id);
            return NotFound();
        }

        if (includePublisher)
        {
            var bookDto =_bookMapper.ToBookWithPublisherDto(book);
            return Ok(bookDto);
        }
        var bookWithPublisherDto = _bookMapper.ToBookWithoutPublisherDto( book);
        return Ok(bookWithPublisherDto);
    }
    [HttpPost]
    [Authorize(Policy = "AdminCanAccess")]
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
        var bookReadDto = _bookMapper.ToBookReadDto(createdBook);
        return CreatedAtRoute("GetBookById", new { id = bookReadDto.Id,includePublisher=false }, bookReadDto);
    }

    [HttpDelete("{id}")]
    [Authorize(Policy = "AdminCanAccess")]
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
    [Authorize(Policy = "AdminCanAccess")]
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
   // [Authorize(Policy = "AdminCanAccess")]
    public async Task<IActionResult> PartiallyUpdateBookAsync(Guid id, [FromBody] JsonPatchDocument<BookUpdateDto> patchDocument)
    {
        var existingBook = await _bookService.GetByIdAsync(id);

        if (existingBook is null)
        {
            _logger.LogDebug("Book with id: {id} was not found", id);
            return NotFound();
        }

        var bookToPatchDto = _bookMapper.ToBookUpdateDto( existingBook);
        patchDocument.ApplyTo(bookToPatchDto, ModelState);

        if(!TryValidateModel(bookToPatchDto))
        {
            return ValidationProblem(ModelState);
        }

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

      var bookForUpdate = _bookMapper.UpdateFromDto(existingBook, bookToPatchDto);
        await _bookService.PartiallyUpdateBookAsync(bookForUpdate);
        return NoContent();
    }

    [HttpOptions()]
    public IActionResult GetBooksOptions()
    {
        Response.Headers.Add("Allow", "GET,HEAD,OPTIONS,POST");
        return Ok();
    }
    [HttpOptions("{id}")]
    public IActionResult GetBookOptionsWithId(Guid id)
    {
        Response.Headers.Add("Allow", "GET,PATCH,PUT");
        return Ok();
    }
    // will use this method to return 422 UnprocessableEntity response
    public override ActionResult ValidationProblem(
        ModelStateDictionary modelStateDictionary)
    {
        var options = HttpContext.RequestServices
            .GetRequiredService<IOptions<ApiBehaviorOptions>>();
        return (ActionResult)options.Value.InvalidModelStateResponseFactory(ControllerContext);
    }

}
