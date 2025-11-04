using LibraryManagement.WebAPI.Data;
using LibraryManagement.WebAPI.Helpers;
using LibraryManagement.WebAPI.Models;
using LibraryManagement.WebAPI.Models.Common;
using LibraryManagement.WebAPI.Models.Dtos;
using LibraryManagement.WebAPI.Services.Interfaces;
using LibraryManagement.WebAPI.Services.ORM.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace LibraryManagement.WebAPI.Services.Implementations;
public class PublisherService : IPublisherService
{
    private readonly LibraryDbContext _context;
    private readonly IPublisherMapper _publisherMapper;
    public PublisherService(LibraryDbContext context, IPublisherMapper publisherMapper)
    {
        _context = context ?? throw new ArgumentNullException(nameof(context));
        _publisherMapper = publisherMapper ?? throw new ArgumentNullException(nameof(publisherMapper));
       
    }
    public async Task<Publisher?> CreatePublisherAsync(PublisherCreateDto publisherCreateDto)
    {
        var publisher =  _publisherMapper.ToPublisher(publisherCreateDto);
        if (publisherCreateDto == null)
        {
            throw new ArgumentNullException(nameof(publisherCreateDto));
        }
        
        _context.Publishers.Add(publisher);
        await _context.SaveChangesAsync();
        return  publisher;
    }

    public async Task<Publisher> DeletePublisherAsync(Guid id)
    {
        var publisher = _context.Publishers.FirstOrDefault(x => x.Id == id);
        if (publisher == null)
        {
            throw new ArgumentNullException(nameof(publisher));
        }
        _context.Publishers.Remove(publisher);
        await _context.SaveChangesAsync();
        return publisher;
    }

    public async Task<bool> EntityExistAsync(Guid id)
    {
        return await _context.Publishers.AnyAsync(x => x.Id == id);
    }

    public async Task<Publisher?> GetPublisherByEmailAsync(string email)
    {
        var publisher = await _context.Publishers.FirstOrDefaultAsync(x => x.Email == email);
        if (publisher == null)
        {
            throw new ArgumentNullException(nameof(publisher));
        }
        return publisher;
    }

    public async Task<Publisher?> GetPublisherByIdAsync(Guid id)
    {
        var publisher =await  _context.Publishers
                                    .AsNoTracking()
                                    .FirstOrDefaultAsync(x => x.Id == id);
        if (publisher == null)
        {
            throw new ArgumentNullException(nameof(publisher));
        }
        return  publisher;
    }

    public async Task<IEnumerable<PublisherReadDto>> ListAllPublisherAsync(QueryOptions queryOptions)
    {
        var query =  _context.Publishers
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
        return await PaginatedResponse<PublisherReadDto>.CreateAsync(result, queryOptions.PageNumber, queryOptions.PageSize);

    }

    public async Task<Publisher> UpdatePublisherAsync(Guid id, PublisherUpdateDto publisherUpdateDto)
    {
        var publisher =  _context.Publishers.FirstOrDefault(x => x.Id == id);
        if (publisher == null)
        {
            throw new ArgumentNullException(nameof(publisher));
        }
        _publisherMapper.UpdateFromDto(publisher, publisherUpdateDto);
        await _context.SaveChangesAsync();
        return publisher;
    }
}

