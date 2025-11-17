using Asp.Versioning;
using LibraryManagement.WebAPI.Models.Common;
using LibraryManagement.WebAPI.Models.Dtos;
using LibraryManagement.WebAPI.Services.Interfaces;
using LibraryManagement.WebAPI.Services.ORM.Interfaces;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Infrastructure;
using Microsoft.EntityFrameworkCore;

namespace LibraryManagement.WebAPI.Controllers;

[Route("api/v{version:ApiVersion}/[controller]")]
[ApiVersion("1.0")]
[ApiController]
public class AuthorsController : ControllerBase
{
    private readonly IAuthorsService _authorService;
    private readonly ILogger<AuthorsController> _logger;
    private readonly IAuthorsMapper _authorMapper;
    private readonly ProblemDetailsFactory _problemDetailsFactory;
    public AuthorsController(IAuthorsService authorService, ILogger<AuthorsController> logger, IAuthorsMapper authorMapper, ProblemDetailsFactory problemDetailsFactory)
    {
        _authorService = authorService ?? throw new ArgumentNullException(nameof(authorService));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _authorMapper = authorMapper ?? throw new ArgumentNullException(nameof(authorMapper));
        _problemDetailsFactory = problemDetailsFactory ?? throw new ArgumentNullException(nameof(problemDetailsFactory));
    }

    [HttpGet]
    public async Task<IActionResult> GetAllAuthors([FromQuery] QueryOptions queryOptions)
    {
        var authors = await _authorService.ListAllAuthorsAsync(queryOptions);
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

        var authorDto = _authorMapper.ToAuthorReadDto(author);

        return Ok(authorDto);
    }
    [HttpPost]
    public async Task<IActionResult> CreateAuthor([FromBody] AuthorCreateDto authorCreateDto)
    {
        if (authorCreateDto == null)
        {
            _logger.LogWarning("CreateAuthor called with null AuthorCreateDto");
            return BadRequest("Author data is required.");
        }
        if (!ModelState.IsValid)
        {
            _logger.LogWarning("Invalid model state for AuthorCreateDto: {ModelState}", ModelState);
            return BadRequest(ModelState);
        }
        var createdAuthor = await _authorService.CreateAuthorAsync(authorCreateDto);
        var authorDto = _authorMapper.ToAuthorReadDto(createdAuthor);
        return CreatedAtAction(nameof(GetAuthorById), new { id = authorDto.Id }, authorDto);
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteAuthor(Guid id)
    {
        var author = await _authorService.GetAuthorByIdAsync(id);
        if (author == null)
        {
            _logger.LogWarning("Author with ID {AuthorId} not found for deletion.", id);
            return NotFound($"Author with ID {id} not found.");
        }
        await _authorService.DeleteAuthorAsync(id);
        return NoContent();
    }

    [HttpPut("{id:guid}")]
    public async Task<IActionResult> UpdateAuthor([FromRoute] Guid id, [FromBody] AuthorUpdateDto authorUpdateDto)
    {
        if (authorUpdateDto == null)
        {
            _logger.LogWarning("UpdateAuthor called with null AuthorUpdateDto");
            return BadRequest("Author data is required.");
        }

        if (!ModelState.IsValid)
        {
            _logger.LogWarning("Invalid model state for AuthorUpdateDto: {ModelState}", ModelState);
            return BadRequest(ModelState);
        }
        try
        {
            var updatedAuthor = await _authorService.UpdateAuthorAsync(id, authorUpdateDto);
            var authorReadDto = _authorMapper.ToAuthorReadDto(updatedAuthor);

            return Ok(authorReadDto);
        }
        catch (DbUpdateConcurrencyException ex)
        {
            _logger.LogError(ex, "Concurrency error occurred while updating author with ID {AuthorId}", id);
            var problemDetails = _problemDetailsFactory.CreateProblemDetails(
                HttpContext,
                statusCode: 409,
                title: "Concurrency Conflict",
                detail: "The record you attempted to edit was modified by another user after you got the original value. The edit operation was canceled."
            );
            return Conflict(problemDetails);
        }
    }
}