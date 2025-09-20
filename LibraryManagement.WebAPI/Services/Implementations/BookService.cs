using LibraryManagement.WebAPI.Data;
using LibraryManagement.WebAPI.Models;
using LibraryManagement.WebAPI.Models.Dtos;
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
    public async Task<Book> CreateBookAsync(BookCreateDto bookCreateDto)
    {
        if (bookCreateDto == null)
        {
            throw new ArgumentNullException(nameof(bookCreateDto));
        }

        var book = new Book()
        {
            Title = bookCreateDto.Title,
            Description = bookCreateDto.Description,
            CoverImageUrl = bookCreateDto.CoverImageUrl,
            Genre = bookCreateDto.Genre,
            PublishedDate = bookCreateDto.PublishedDate,
            Pages = bookCreateDto.Pages,
            PublisherId = bookCreateDto.PublisherId
        };

        foreach (var authorId in bookCreateDto.AuthorIds)
        {
            book.BookAuthors.Add(new BookAuthor()
            {
                AuthorId = authorId,
                BookId = book.Id
            });
        }
        _dbContext.Books.Add(book);
        await SaveChangesAsync();
        return book;
    }

    public Task<bool?> DeleteByIdAsync(Guid id)
    {
        throw new NotImplementedException();
    }

    public async Task<bool> EntityExistAsync(Guid id)
    {
         return await _dbContext.Books.AnyAsync(b =>b.Id == id);
    }

    public async Task<Book?> GetByIdAsync(Guid id, bool includePublisher = false)
    {
       if(includePublisher)
       {
        return  await _dbContext.Books
             .Include(b => b.BookAuthors)
             .ThenInclude(ba => ba.Author)
             .Include(b => b.Publisher)
             .AsNoTracking()
             .FirstOrDefaultAsync(b => b.Id == id);
        }
         return await _dbContext.Books
                .Include(b => b.BookAuthors)
                .ThenInclude(ba => ba.Author)
                .AsNoTracking()
                .FirstOrDefaultAsync(b => b.Id == id);
    }

    public async Task<IEnumerable<Book>> ListAllBooksAsync()
    {
        var books = await _dbContext.Books
            .Include(a => a.BookAuthors)
            .ThenInclude(ba => ba.Author)
            .Include(b => b.Publisher)
            .AsNoTracking()
            .ToListAsync();
        return books;
    }

    public Task<Book> UpdateBookAsync(Book book)
    {
        throw new NotImplementedException();
    }
    public async Task<bool> SaveChangesAsync()
    {
        return (await _dbContext.SaveChangesAsync() >= 0);
    }
}

