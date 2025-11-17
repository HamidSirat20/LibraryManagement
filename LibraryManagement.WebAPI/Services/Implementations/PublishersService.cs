using LibraryManagement.WebAPI.Data;
using LibraryManagement.WebAPI.Helpers;
using LibraryManagement.WebAPI.Models;
using LibraryManagement.WebAPI.Models.Common;
using LibraryManagement.WebAPI.Models.Dtos;
using LibraryManagement.WebAPI.Services.Interfaces;
using LibraryManagement.WebAPI.Services.ORM.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace LibraryManagement.WebAPI.Services.Implementations;
public class PublishersService : IPublishersService
{
    private readonly LibraryDbContext _context;
    private readonly IPublishersMapper _publisherMapper;
    private readonly ILogger<PublishersService> _logger;

    public PublishersService(
        LibraryDbContext context,
        IPublishersMapper publisherMapper,
        ILogger<PublishersService> logger)
    {
        _context = context ?? throw new ArgumentNullException(nameof(context));
        _publisherMapper = publisherMapper ?? throw new ArgumentNullException(nameof(publisherMapper));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public async Task<Publisher?> CreatePublisherAsync(PublisherCreateDto publisherCreateDto)
    {
        using var _ = _logger.BeginScope("Creating publisher");

        try
        {
            if (publisherCreateDto == null)
            {
                _logger.LogWarning("CreatePublisherAsync called with null DTO");
                throw new ArgumentNullException(nameof(publisherCreateDto));
            }

            var publisher = _publisherMapper.ToPublisher(publisherCreateDto);

            _context.Publishers.Add(publisher);
            await _context.SaveChangesAsync();

            _logger.LogInformation("Creating new publisher with ID: {PublisherId}", publisher.Id);
            return publisher;
        }
        catch (DbUpdateException ex)
        {
            _logger.LogError(ex, "Database error creating publisher ");
            throw new InvalidOperationException("Unable to create publisher due to database error.", ex);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unexpected error creating publisher");
            throw;
        }
    }

    public async Task<Publisher> DeletePublisherAsync(Guid id)
    {
        using var _ = _logger.BeginScope("Deleting publisher {PublisherId}", id);

        try
        {
            var publisher = await _context.Publishers.FirstOrDefaultAsync(x => x.Id == id);
            if (publisher == null)
            {
                _logger.LogWarning("Publisher with ID {PublisherId} not found for deletion", id);
                throw new ArgumentException($"Publisher not found.");
            }

            var hasBooks = await _context.Books.AnyAsync(b => b.PublisherId == id);
            if (hasBooks)
            {
                _logger.LogWarning("Cannot delete publisher {PublisherId} with {BookCount} books",
                    id, publisher.Books.Count);
                throw new InvalidOperationException("Cannot delete publisher that has associated books.");
            }

            _context.Publishers.Remove(publisher);
            await _context.SaveChangesAsync();

            _logger.LogInformation("Deleted publisher with ID: {PublisherId}",
                 id);
            return publisher;
        }
        catch (DbUpdateException ex)
        {
            _logger.LogError(ex, "Database error deleting publisher {PublisherId}", id);
            throw new InvalidOperationException("Unable to delete publisher due to database error.", ex);
        }
        catch (Exception ex) when (ex is ArgumentException || ex is InvalidOperationException)
        {
            throw;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unexpected error deleting publisher {PublisherId}", id);
            throw;
        }
    }

    public async Task<bool> EntityExistAsync(Guid id)
    {
        try
        {
            return await _context.Publishers.AnyAsync(x => x.Id == id);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error checking publisher existence {PublisherId}", id);
            throw;
        }
    }

    public async Task<Publisher?> GetPublisherByEmailAsync(string email)
    {
        if (string.IsNullOrWhiteSpace(email))
        {
            _logger.LogWarning("GetPublisherByEmailAsync called with empty email");
            throw new ArgumentException("Email cannot be null or empty.");
        }

        try
        {
            var publisher = await _context.Publishers
                .FirstOrDefaultAsync(x => x.Email == email);

            if (publisher == null)
            {
                return null;
            }
            return publisher;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving publisher by email: {Email}", email);
            throw;
        }
    }

    public async Task<Publisher?> GetPublisherByIdAsync(Guid id)
    {
        try
        {
            var publisher = await _context.Publishers
                .AsNoTracking()
                .FirstOrDefaultAsync(x => x.Id == id);

            if (publisher == null)
            {
                return null;
            }
            return publisher;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving publisher {PublisherId}", id);
            throw;
        }
    }

    public async Task<IEnumerable<PublisherReadDto>> ListAllPublisherAsync(QueryOptions queryOptions)
    {
        using var _ = _logger.BeginScope("Listing publishers");

        try
        {
            if (queryOptions == null)
            {
                _logger.LogWarning("ListAllPublisherAsync called with null query options");
                throw new ArgumentNullException(nameof(queryOptions));
            }

            var query = _context.Publishers
                .AsNoTracking()
                .AsQueryable();

            // Apply search term filtering
            var searchTerm = queryOptions.SearchTerm?.Trim() ?? string.Empty;
            if (!string.IsNullOrWhiteSpace(searchTerm))
            {
                query = query.Where(p => p.Name.Contains(searchTerm, StringComparison.OrdinalIgnoreCase) ||
                                         p.Email.Contains(searchTerm, StringComparison.OrdinalIgnoreCase) ||
                                         p.Address.Contains(searchTerm, StringComparison.OrdinalIgnoreCase));
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

            var result = query.Select(p => new PublisherReadDto
            {
                Id = p.Id,
                Name = p.Name,
                Email = p.Email,
                Address = p.Address,
                Website = p.Website,
                BookCount = p.Books.Count()
            });

            var paginatedResult = await PaginatedResponse<PublisherReadDto>.CreateAsync(
                result, queryOptions.PageNumber, queryOptions.PageSize);

            _logger.LogInformation("Listed {PublisherCount} publishers (Page {PageNumber})",
                paginatedResult.Count(), queryOptions.PageNumber);

            return paginatedResult;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error listing publishers");
            throw;
        }
    }

    public async Task<Publisher> UpdatePublisherAsync(Guid id, PublisherUpdateDto publisherUpdateDto)
    {
        using var _ = _logger.BeginScope("Updating publisher {PublisherId}", id);

        try
        {
            if (publisherUpdateDto == null)
            {
                _logger.LogWarning("UpdatePublisherAsync called with null DTO for publisher {PublisherId}", id);
                throw new ArgumentNullException(nameof(publisherUpdateDto));
            }

            var publisher = await _context.Publishers.FirstOrDefaultAsync(x => x.Id == id);
            if (publisher == null)
            {
                _logger.LogWarning("Publisher with ID {PublisherId} not found for update", id);
                throw new ArgumentException($"Publisher not found.");
            }

            _publisherMapper.UpdateFromDto(publisher, publisherUpdateDto);
            await _context.SaveChangesAsync();

            _logger.LogInformation("Updated publisher with ID: {PublisherId}",
                 id);
            return publisher;
        }
        catch (DbUpdateException ex)
        {
            _logger.LogError(ex, "Database error updating publisher {PublisherId}", id);
            throw new InvalidOperationException("Unable to update publisher due to database error.", ex);
        }
        catch (Exception ex) when (ex is ArgumentException || ex is ArgumentNullException)
        {
            throw;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unexpected error updating publisher {PublisherId}", id);
            throw;
        }
    }
}