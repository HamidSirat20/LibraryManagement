using LibraryManagement.WebAPI.Data;
using LibraryManagement.WebAPI.Helpers;
using LibraryManagement.WebAPI.Models;
using LibraryManagement.WebAPI.Models.Common;
using LibraryManagement.WebAPI.Models.Dtos;
using LibraryManagement.WebAPI.Services.Interfaces;
using LibraryManagement.WebAPI.Services.ORM.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace LibraryManagement.WebAPI.Services.Implementations;
public class AuthorsService : IAuthorsService
{
    private readonly LibraryDbContext _context;
    private readonly ILogger<AuthorsService> _logger;
    private readonly IAuthorsMapper authorMapper;
    public AuthorsService(LibraryDbContext context, ILogger<AuthorsService> logger, IAuthorsMapper authorMapper)
    {
        _context = context ?? throw new ArgumentNullException(nameof(context));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        this.authorMapper = authorMapper ?? throw new ArgumentNullException(nameof(authorMapper));
    }

    public async Task<Author?> CreateAuthorAsync(AuthorCreateDto authorCreateDto)
    {
        using var _ = _logger.BeginScope("Creating author");
        try
        {
            if (authorCreateDto == null)
            {
                _logger.LogWarning("CreateAuthorAsync called with null DTO");
                throw new ArgumentNullException(nameof(authorCreateDto));
            }
            var author = authorMapper.ToAuthor(authorCreateDto);
            _context.Authors.Add(author);
            await _context.SaveChangesAsync();
            _logger.LogInformation("Creating new author with ID: {AuthorId}", author.Id);
            return author;

        }
        catch (DbUpdateException ex)
        {
            _logger.LogError(ex, "Database error creating author ");
            throw new InvalidOperationException("Unable to create author due to database error.", ex);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating author");
            throw;
        }
    }

    public async Task<Author> DeleteAuthorAsync(Guid id)
    {
        try
        {
            var author = await _context.Authors.FindAsync(id);
            if (author == null)
            {
                _logger.LogWarning("Author with ID {AuthorId} not found for deletion", id);
                throw new KeyNotFoundException($"Author not found");
            }

            var hasBooks = await _context.BookAuthors.AnyAsync(ba => ba.AuthorId == id);
            if (hasBooks)
            {
                _logger.LogWarning("Cannot delete publisher {PublisherId} with {BookCount} books",
                    id, author.BookAuthors.Count);
                throw new InvalidOperationException("Cannot delete publisher that has associated books.");
            }
            _context.Authors.Remove(author);
            await _context.SaveChangesAsync();
            _logger.LogInformation("Deleted author with ID: {AuthorId}", id);
            return author;
        }

        catch (DbUpdateException ex)
        {
            _logger.LogError(ex, "Database error deleting author {AuthorId}", id);
            throw new InvalidOperationException("Unable to delete author due to database error.", ex);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting author");
            throw;
        }
    }

    public async Task<bool> EntityExistsAsync(Guid id)
    {
        var exists = await _context.Authors.AnyAsync(a => a.Id == id);
        try
        {
            if (!exists)
            {
                _logger.LogWarning("Author with ID {AuthorId} does not exist", id);
            }
            return exists;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error checking if author exists");
            throw;
        }
    }

    public async Task<Author?> GetAuthorByEmailAsync(string email)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(email))
            {
                _logger.LogWarning("GetAuthorByEmailAsync called with null or empty email");
                throw new ArgumentException("Email cannot be null or empty");
            }
            var author = await _context.Authors
                            .AsNoTracking()
                            .FirstOrDefaultAsync(a => a.Email.ToLower() == email.ToLower());
            if (author == null)
            {
                _logger.LogWarning("Author with email {Email} not found", email);
                return null;
            }
            return author;

        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting author by email");
            throw;
        }
    }

    public async Task<Author?> GetAuthorByIdAsync(Guid id)
    {
        try
        {
            var author = await _context.Authors
                        .Include(p => p.BookAuthors)
                        .AsNoTracking()
                        .FirstOrDefaultAsync(a => a.Id == id);
            if (author == null)
            {
                _logger.LogWarning("Author with ID {AuthorId} not found", id);
                return null;
            }
            return author;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting author by ID");
            throw;
        }

    }

    public async Task<PaginatedResponse<AuthorReadDto>> ListAllAuthorsAsync(QueryOptions queryOptions)
    {
        using var _ = _logger.BeginScope("Listing authors");
        try
        {
            if (queryOptions == null)
            {
                _logger.LogWarning("ListAllAuthorAsync called with null query options");
                throw new ArgumentNullException(nameof(queryOptions));
            }
            var query = _context.Authors
                        .AsNoTracking()
                        .AsQueryable();

            // Apply search term filtering
            var searchTerm = queryOptions.SearchTerm?.Trim() ?? string.Empty;
            if (!string.IsNullOrWhiteSpace(searchTerm))
            {
                query = query.Where(p => p.FirstName.Contains(searchTerm, StringComparison.OrdinalIgnoreCase) ||
                                                   p.Email.Contains(searchTerm, StringComparison.OrdinalIgnoreCase) ||
                                                   p.LastName.Contains(searchTerm, StringComparison.OrdinalIgnoreCase) ||
                                                   p.Bio.Contains(searchTerm, StringComparison.OrdinalIgnoreCase));
            }
            // Apply sorting
            if (!string.IsNullOrWhiteSpace(queryOptions.OrderBy))
            {
                query = query.ApplySorting(queryOptions.OrderBy, queryOptions.IsDescending, "Name");
            }
            else
            {

                query = query.ApplySorting(null, false, "Title");
            }
            var result = query.Select(p => new AuthorReadDto
            {
                Id = p.Id,
                FirstName = p.FirstName,
                LastName = p.LastName,
                Email = p.Email,
                Bio = p.Bio,
                CreatedAt = p.CreatedAt,
                UpdatedAt = p.UpdatedAt,
                BookCount = p.BookAuthors.Count()
            });
            return await PaginatedResponse<AuthorReadDto>.CreateAsync(result, queryOptions.PageNumber, queryOptions.PageSize);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error listing authors");
            throw;
        }

    }

    public async Task<PaginatedResponse<AuthorWithBooksDto>> ListAllAuthorsWithBooksAsync(QueryOptions queryOptions)
    {
        using var _ = _logger.BeginScope("Listing authors");
        try
        {
            if (queryOptions == null)
            {
                _logger.LogWarning("ListAllAuthorAsync called with null query options");
                throw new ArgumentNullException(nameof(queryOptions));
            }
            var query = _context.Authors
                         .Include(a => a.BookAuthors)
                        .AsNoTracking()
                        .AsQueryable();

            // Apply search term filtering
            var searchTerm = queryOptions.SearchTerm?.Trim() ?? string.Empty;
            if (!string.IsNullOrWhiteSpace(searchTerm))
            {
                query = query.Where(p => p.FirstName.Contains(searchTerm, StringComparison.OrdinalIgnoreCase) ||
                                                   p.Email.Contains(searchTerm, StringComparison.OrdinalIgnoreCase) ||
                                                   p.LastName.Contains(searchTerm, StringComparison.OrdinalIgnoreCase) ||
                                                   p.Bio.Contains(searchTerm, StringComparison.OrdinalIgnoreCase));
            }
            // Apply sorting
            if (!string.IsNullOrWhiteSpace(queryOptions.OrderBy))
            {
                query = query.ApplySorting(queryOptions.OrderBy, queryOptions.IsDescending, "Name");
            }
            else
            {

                query = query.ApplySorting(null, false, "Title");
            }
            var result = query.Select(p => new AuthorWithBooksDto
            {
                Id = p.Id,
                FirstName = p.FirstName,
                LastName = p.LastName,
                Email = p.Email,
                Bio = p.Bio,
                CreatedAt = p.CreatedAt,
                UpdatedAt = p.UpdatedAt,
                Books = p.BookAuthors
                         .Select(ba => new BookReadDto
                         {
                             Id = ba.Book.Id,
                             Title = ba.Book.Title,
                             Genre = ba.Book.Genre,
                             Description = ba.Book.Description,
                             CoverImageUrl = ba.Book.CoverImageUrl,
                             PublishedDate = ba.Book.PublishedDate,
                             Pages = ba.Book.Pages
                         }).ToList()
            });
            return await PaginatedResponse<AuthorWithBooksDto>.CreateAsync(result, queryOptions.PageNumber, queryOptions.PageSize);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error listing authors");
            throw;
        }
    }

    public async Task<Author> UpdateAuthorAsync(Guid id, AuthorUpdateDto authorUpdateDto)
    {
        using var _ = _logger.BeginScope("Updating author");
        try
        {
            var author = await _context.Authors
                            .Include(a => a.BookAuthors)
                            .ThenInclude(ba => ba.Book)
                            .FirstOrDefaultAsync(a => a.Id == id);
            if (author == null)
            {
                _logger.LogWarning("Author with ID {AuthorId} not found for update", id);
                throw new KeyNotFoundException($"Author not found");
            }

            var updatedAuthor = authorMapper.UpdateFromDto(author, authorUpdateDto);
            await _context.SaveChangesAsync();
            _logger.LogInformation("Updated author with ID: {AuthorId}", id);
            return updatedAuthor;
        }
        catch (DbUpdateException ex)
        {
            _logger.LogError(ex, "Database error updating author {AuthorId}", id);
            throw new InvalidOperationException("Unable to update author due to database error.", ex);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating author");
            throw;

        }
    }
}
