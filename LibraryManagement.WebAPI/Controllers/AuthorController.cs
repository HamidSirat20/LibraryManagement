using Asp.Versioning;
using LibraryManagement.WebAPI.Models.Common;
using LibraryManagement.WebAPI.Services.Interfaces;
using LibraryManagement.WebAPI.Services.ORM.Interfaces;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace LibraryManagement.WebAPI.Controllers
{
    [Route("api/v{version:ApiVersion}/[controller]")]
    [ApiVersion("1.0")]
    [ApiController]
    public class AuthorController : ControllerBase
    {
        private readonly IAuthorService _authorService;
        private readonly ILogger<AuthorController> _logger;
        private readonly IAuthorMapper _authorMapper;
        public AuthorController(IAuthorService authorService, ILogger<AuthorController> logger, IAuthorMapper authorMapper)
        {
            _authorService = authorService ?? throw new ArgumentNullException(nameof(authorService));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _authorMapper = authorMapper ?? throw new ArgumentNullException(nameof(authorMapper));
        }

        [HttpGet]
        public async Task<IActionResult> GetAllAuthors([FromQuery] QueryOptions queryOptions)
        {
            var authors = await _authorService.ListAllAuthorAsync(queryOptions);
            if (!authors.Any())
            {
                _logger.LogInformation("No authors found with the provided query options.");
                return NotFound("No authors found.");
            }
            var paginationMetadata = new
            {
                TotalCount = authors.TotalRecords,
                PageSize = authors.PageSize,
                CurrentPage = authors.CurrentPage,
                TotalPages = authors.TotalPages,
                HasNext = authors.HasNext,
                HasPrevious = authors.HasPrevious
            };
            Response.Headers.Add("X-Pagination", System.Text.Json.JsonSerializer.Serialize(paginationMetadata));
            return Ok(authors);
        }
        [HttpGet("{id}")]
        public async Task<IActionResult> GetAuthorById(Guid id)
        {
            var author = await _authorService.GetAuthorByIdAsync(id);
            if (author == null)
            {
                _logger.LogWarning("Author with ID {AuthorId} not found.", id);
                return NotFound($"Author with ID {id} not found.");
            }
            var bookCount =  author.BookAuthors.Select(b=>b.Book).Count();

            var authorDto = _authorMapper.ToAuthorReadDto(author);
            authorDto.BookCount = bookCount;
            return Ok(authorDto);

        }
    }
}
