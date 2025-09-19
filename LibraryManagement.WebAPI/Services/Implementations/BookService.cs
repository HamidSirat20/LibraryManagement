using LibraryManagement.WebAPI.Data;
using LibraryManagement.WebAPI.Models;
using LibraryManagement.WebAPI.Services.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace LibraryManagement.WebAPI.Services.Implementations;

public class BookService : IBookService
{
    private readonly LibraryDbContext _dbContext;

    public BookService(LibraryDbContext dbContext)
    {
        _dbContext = dbContext ?? throw new ArgumentNullException(nameof(dbContext));
    }
    public Task<Book> CreateEntityAsync(Book book)
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

    public async Task<Book?> GetByIdAsync(Guid id, bool includePublisher = false)
    {
        var book = await _dbContext.Books
             .Include(b => b.BookAuthors)
             .ThenInclude(ba => ba.Author)
             .Include(b => b.Publisher)
             .FirstOrDefaultAsync(b=>b.Id ==id);
        return book;
    }

    public async Task<IEnumerable<Book>> ListAllBooksAsync()
    {
        var books = await _dbContext.Books
            .Include(a => a.BookAuthors)
            .ThenInclude(ba => ba.Author)
            .Include(b => b.Publisher)
            .AsNoTracking().ToListAsync();
        return books;
    }

    public Task<Book> UpdateEntityAsync(Book book)
    {
        throw new NotImplementedException();
    }
}

