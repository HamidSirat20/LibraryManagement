using Asp.Versioning;
using LibraryManagement.WebAPI.ActionConstraints;
using LibraryManagement.WebAPI.Helpers;
using LibraryManagement.WebAPI.Models;
using LibraryManagement.WebAPI.Models.Common;
using LibraryManagement.WebAPI.Models.Dtos;
using LibraryManagement.WebAPI.Models.Dtos.Common;
using LibraryManagement.WebAPI.Services.Interfaces;
using LibraryManagement.WebAPI.Services.ORM.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.JsonPatch;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Infrastructure;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.Extensions.Options;
using System.Text.Json;

namespace LibraryManagement.WebAPI.Controllers;

[ApiController]
//[Authorize]
//[ResponseCache(CacheProfileName = "120SecondsCacheProfile")]
[Route("api/v{version:ApiVersion}/[controller]")]
[ApiVersion("1.0")]
[ProducesResponseType(StatusCodes.Status401Unauthorized)]
[ProducesResponseType(StatusCodes.Status404NotFound)]
[ProducesResponseType(StatusCodes.Status405MethodNotAllowed)]
[ProducesResponseType(StatusCodes.Status400BadRequest)]
public class BooksController : ControllerBase
{
    private readonly ILogger<BooksController> _logger;
    private readonly IBooksService _bookService;
    private readonly IBooksMapper _bookMapper;
    private readonly IPropertyCheckerService _propertyCheckerService;
    private readonly ProblemDetailsFactory _problemDetailsFactory;

    public BooksController(ILogger<BooksController> logger, IBooksService bookService, IBooksMapper bookMapper, IPropertyCheckerService propertyCheckerService, ProblemDetailsFactory problemDetailsFactory)
    {
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _bookService = bookService ?? throw new ArgumentNullException(nameof(bookService));
        _bookMapper = bookMapper ?? throw new ArgumentNullException(nameof(bookMapper));
        _propertyCheckerService = propertyCheckerService ?? throw new ArgumentNullException(nameof(propertyCheckerService));
        _problemDetailsFactory = problemDetailsFactory ?? throw new ArgumentNullException(nameof(problemDetailsFactory));
    }

    [Produces("application/vnd.hamid.hateoas+json")]
    [RequestHeaderMatchesMediaTypeAttribute("Accept",
          "application/vnd.hamid.hateoas+json")]
    [ApiExplorerSettings(IgnoreApi = true)]
    [HttpGet(Name = "GetAllBooks")]
    public async Task<IActionResult> ListAllBooksWithLinks([FromQuery] QueryOptions queryOptions)
    {
        var books = await _bookService.ListAllBooksAsync(queryOptions);

        if (!_propertyCheckerService.TypeHasProperties<BookWithPublisherDto>(queryOptions.Fields))
        {
            return BadRequest(_problemDetailsFactory.CreateProblemDetails(
                HttpContext,
                statusCode: 400,
                title: "Bad request",
                detail: $"The fields {queryOptions.Fields} are invalid."));

        }

        // var paginationMetadata 
        var paginationMetadata = new
        {
            totalCount = books.TotalRecords,
            pageSize = books.PageSize,
            currentPage = books.CurrentPage,
            totalPages = books.TotalPages,
        };
        Response.Headers["X-Pagination"] = JsonSerializer.Serialize(paginationMetadata);

        var bookWithPublisherDto = new List<BookWithPublisherDto>();
        foreach (var book in books)
        {
            bookWithPublisherDto.Add(_bookMapper.ToBookWithPublisherDto(book));
        }

        //links 
        var links = CreateLinksForBooks(queryOptions, books.HasPrevious, books.HasNext);
        var shapedBooks = bookWithPublisherDto.ShapeFields(queryOptions.Fields);
        var shapedBooksWithLinks = shapedBooks.Select(book =>
        {
            var bookAsDictionary = book as IDictionary<string, object>;
            var bookLinks = CreateLinksForBook((Guid)bookAsDictionary["Id"], queryOptions.Fields);
            bookAsDictionary.Add("links", bookLinks);
            return bookAsDictionary;

        });


        var result = new
        {
            value = shapedBooksWithLinks,
            links
        };
        return Ok(result);
    }

    [Produces("application/json", "application/xml")]
    [RequestHeaderMatchesMediaTypeAttribute("Accept",
         "application/json", "application/xml")]
    [HttpGet(Name = "GetAllBooks")]
    public async Task<IActionResult> ListAllBooks([FromQuery] QueryOptions queryOptions)
    {
        var books = await _bookService.ListAllBooksAsync(queryOptions);
        if (!_propertyCheckerService.TypeHasProperties<BookWithPublisherDto>(queryOptions.Fields))
        {
            return BadRequest(_problemDetailsFactory.CreateProblemDetails(
                HttpContext,
                statusCode: 400,
                title: "Bad request",
                detail: $"The fields {queryOptions.Fields} are invalid."));
        }

        // var paginationMetadata 
        var paginationMetadata = new
        {
            totalCount = books.TotalRecords,
            pageSize = books.PageSize,
            currentPage = books.CurrentPage,
            totalPages = books.TotalPages,
        };

        var bookWithPublisherDto = new List<BookWithPublisherDto>();
        foreach (var book in books)
        {
            bookWithPublisherDto.Add(_bookMapper.ToBookWithPublisherDto(book));
        }

        Response.Headers["X-Pagination"] = JsonSerializer.Serialize(paginationMetadata);
        var shapedBooks = bookWithPublisherDto.ShapeFields(queryOptions.Fields);
        return Ok(shapedBooks);

    }
    private string? GenerateBooksResourceUri(QueryOptions queryOptions, ResourceUriType type)
    {
        switch (type)
        {
            case ResourceUriType.PreviousPage:
                return Url.Link(ApiRoutes.Books.GetAllBooks,
                    new
                    {
                        Fields = queryOptions.Fields,
                        page = queryOptions.PageNumber - 1,
                        size = queryOptions.PageSize,
                        search = queryOptions.SearchTerm,
                        genre = queryOptions.Genre,
                        sort = queryOptions.OrderBy,
                        desc = queryOptions.IsDescending
                    });
            case ResourceUriType.NextPage:
                return Url.Link(ApiRoutes.Books.GetAllBooks,
                    new
                    {
                        Fields = queryOptions.Fields,
                        page = queryOptions.PageNumber + 1,
                        size = queryOptions.PageSize,
                        search = queryOptions.SearchTerm,
                        genre = queryOptions.Genre,
                        sort = queryOptions.OrderBy,
                        desc = queryOptions.IsDescending
                    });
            case ResourceUriType.Current:
            default:
                return Url.Link(ApiRoutes.Books.GetAllBooks,
                    new
                    {
                        Fields = queryOptions.Fields,
                        page = queryOptions.PageNumber,
                        size = queryOptions.PageSize,
                        search = queryOptions.SearchTerm,
                        genre = queryOptions.Genre,
                        sort = queryOptions.OrderBy,
                        desc = queryOptions.IsDescending
                    });
        }
    }

    [Produces("application/vnd.hamid.hateoas+json")]
    [RequestHeaderMatchesMediaTypeAttribute("Accept",
           "application/vnd.hamid.hateoas+json")]
    [ApiExplorerSettings(IgnoreApi = true)]
    [HttpGet("{id}", Name = "GetBook")]
    public async Task<IActionResult> GetBookByIdWithLinks(Guid id, string? fields, [FromQuery] bool includePublisher = false)
    {
        var book = await _bookService.GetByIdAsync(id, includePublisher);
        if (book is null)
        {
            _logger.LogDebug("Book with id: {id} was not found", id);
            return NotFound();
        }

        if (includePublisher)
        {
            var bookDto = _bookMapper.ToBookWithPublisherDto(book);
            return Ok(bookDto);
        }

        if (!_propertyCheckerService.TypeHasProperties<BookWithPublisherDto>(fields))
        {
            return BadRequest(_problemDetailsFactory.CreateProblemDetails(
                HttpContext,
                statusCode: 400,
                title: "Bad request",
                detail: $"The fields {fields} are invalid."));
        }

        var links = CreateLinksForBook(id, fields);
        var bookDict = _bookMapper.ToBookWithoutPublisherDto(book).ShapeField(fields) as IDictionary<string, object>;
        bookDict.Add("links", links);
        return Ok(bookDict);
    }

    [Produces("application/json", "application/xml")]
    [RequestHeaderMatchesMediaTypeAttribute("Accept",
          "application/json", "application/xml")]
    [HttpGet("{id}", Name = "GetBook")]
    public async Task<IActionResult> GetBookById(Guid id, string? fields, [FromQuery] bool includePublisher = false)
    {
        var book = await _bookService.GetByIdAsync(id, includePublisher);
        if (book is null)
        {
            _logger.LogDebug("Book with id: {id} was not found", id);
            return NotFound();
        }

        if (includePublisher)
        {
            var bookDto = _bookMapper.ToBookWithPublisherDto(book);
            return Ok(bookDto);
        }

        if (!_propertyCheckerService.TypeHasProperties<BookWithPublisherDto>(fields))
        {
            return BadRequest(_problemDetailsFactory.CreateProblemDetails(
                HttpContext,
                statusCode: 400,
                title: "Bad request",
                detail: $"The fields {fields} are invalid."));
        }

        var bookReturnDto = _bookMapper.ToBookWithoutPublisherDto(book).ShapeField(fields);
        return Ok(bookReturnDto);

    }

    private IEnumerable<LinkDto> CreateLinksForBook(Guid id, string? fields)
    {
        var links = new List<LinkDto>();
        if (string.IsNullOrWhiteSpace(fields))
        {
            links.Add(new LinkDto(Url.Link(ApiRoutes.Books.GetBook, new { id }), "self", "GET"));
        }
        else
        {
            links.Add(new LinkDto(Url.Link(ApiRoutes.Books.GetBook, new { id, fields }), "self", "GET"));
        }
        links.Add(new LinkDto(Url.Link(ApiRoutes.Books.DeleteBook, new { id }), "delete_book", "DELETE"));
        links.Add(new LinkDto(Url.Link(ApiRoutes.Books.UpdateBook, new { id }), "update_book", "PUT"));
        links.Add(new LinkDto(Url.Link(ApiRoutes.Books.PartiallyUpdateBook, new { id }), "partially_update_book", "PATCH"));
        return links;
    }
    private IEnumerable<LinkDto> CreateLinksForBooks(QueryOptions queryOptions, bool hasPrevious, bool hasNext)
    {
        var links = new List<LinkDto>();
        //self link
        links.Add(
            new LinkDto(GenerateBooksResourceUri(queryOptions, ResourceUriType.Current), "self", "GET")
        );
        //next page link
        if (hasNext)
        {
            links.Add(new LinkDto(GenerateBooksResourceUri(queryOptions, ResourceUriType.NextPage), "nextPage", "GET"));
        }
        //previous page link
        if (hasPrevious)
        {
            links.Add(new LinkDto(GenerateBooksResourceUri(queryOptions, ResourceUriType.PreviousPage), "previousPage", "GET"));
        }
        return links;
    }

    [HttpPost(Name = "CreateBook")]
    /// [Authorize(Policy = "AdminCanAccess")]
    public async Task<IActionResult> CreateBook([FromForm] BookDto bookCreateDto)
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

        var links = CreateLinksForBook(createdBook.Id, null);

        var bookToReturn = _bookMapper.ToBookReadDto(createdBook).ShapeField(null) as IDictionary<string, object?>;
        bookToReturn.Add("links", links);
        return CreatedAtRoute("GetBook", new { id = bookToReturn["Id"], includePublisher = false }, bookToReturn);
    }

    [HttpDelete("{id}", Name = "DeleteBook")]
    [Authorize(Policy = "AdminCanAccess")]
    public async Task<IActionResult> DeleteBookById(Guid id)
    {
        if (!await _bookService.EntityExistsAsync(id))
        {
            _logger.LogDebug("Book with id: {id} was not found", id);
            return NotFound();
        }
        var result = await _bookService.DeleteBookByIdAsync(id);
        if (result == null || result == false)
        {
            _logger.LogError("Something went wrong while deleting the book with id: {id}", id);
            return StatusCode(500, "A problem happened while handling your request.");
        }
        return NoContent();
    }

    [HttpPut("{id}", Name = "UpdateBook")]
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


    [HttpPatch("{id}", Name = "PartiallyUpdateBook")]
    // [Authorize(Policy = "AdminCanAccess")]
    public async Task<IActionResult> PartiallyUpdateBookAsync(Guid id, [FromBody] JsonPatchDocument<BookUpdateDto> patchDocument)
    {
        var existingBook = await _bookService.GetByIdAsync(id);

        if (existingBook is null)
        {
            _logger.LogDebug("Book with id: {id} was not found", id);
            return NotFound();
        }

        var bookToPatchDto = _bookMapper.ToBookUpdateDto(existingBook);
        patchDocument.ApplyTo(bookToPatchDto, ModelState);

        if (!TryValidateModel(bookToPatchDto))
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
    // will use this method to return 422 Unpossessable Entity response
    public override ActionResult ValidationProblem(
        ModelStateDictionary modelStateDictionary)
    {
        var options = HttpContext.RequestServices
            .GetRequiredService<IOptions<ApiBehaviorOptions>>();
        return (ActionResult)options.Value.InvalidModelStateResponseFactory(ControllerContext);
    }

}
