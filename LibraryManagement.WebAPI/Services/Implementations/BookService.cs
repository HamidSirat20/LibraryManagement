using LibraryManagement.WebAPI.Data;
using LibraryManagement.WebAPI.Models;
using LibraryManagement.WebAPI.Models.Dtos;
using LibraryManagement.WebAPI.Services.Interfaces;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Conventions;

namespace LibraryManagement.WebAPI.Services.Implementations;

public class BookService : IBookService
{
    private readonly LibraryDbContext _dbContext;

    public BookService(LibraryDbContext dbContext)
    {
        _dbContext = dbContext;
    }
    public Task<BookReadDto> CreateEntityAsync(Book book)
    {
        throw new NotImplementedException();
    }

    public Task<bool?> DeleteByIdAsync(Guid id)
    {
        throw new NotImplementedException();
    }

    public Task<bool> EntityExistAsync(Book book)
    {
        throw new NotImplementedException();
    }

    public Task<BookReadDto?> GetByIdAsync(Guid id)
    {
        throw new NotImplementedException();
    }

    public Task<BookReadDto?> GetByIdWithAuthorsAsync(Guid id)
    {
        throw new NotImplementedException();
    }

    public async Task<IEnumerable<BookReadDto>> ListAllAsync()
    {
        var bookDtos = await _dbContext.Books
         .Include(b => b.Publisher)
         .AsNoTracking()
         .Select(b => new BookReadDto
         {
             Title = b.Title,
             Description = b.Description,
             CoverImageUrl = b.CoverImageUrl,
             PublishedDate = b.PublishedDate,
             Genre = b.Genre,
             Pages = b.Pages,
             PublisherId = b.PublisherId,
             Publisher = b.Publisher
         })
         .ToListAsync();

        return bookDtos;
    }

    public Task<BookReadDto> UpdateEntityAsync(Book book)
    {
        throw new NotImplementedException();
    }
}

