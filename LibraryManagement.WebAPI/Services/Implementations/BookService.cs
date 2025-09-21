using LibraryManagement.WebAPI.Data;
using LibraryManagement.WebAPI.Models;
using LibraryManagement.WebAPI.Models.Dtos;
using LibraryManagement.WebAPI.Services.Interfaces;
using LibraryManagement.WebAPI.Services.ORM;
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

    public async Task<bool?> DeleteBookByIdAsync(Guid id)
    {
       var book  = await _dbContext.Books.FirstOrDefaultAsync(b => b.Id == id);
        if (book == null) return false;
        _dbContext.Books.Remove(book);
        return await SaveChangesAsync();
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

    public async Task<Book> UpdateBookAsync(Guid id, BookUpdateDto bookUpdateDto)
    {
        var existingBook = await _dbContext.Books
            .Include(b => b.Publisher)
            .Include(b => b.BookAuthors)
            .FirstOrDefaultAsync(b => b.Id == id);

        if (existingBook == null)
            throw new ArgumentException("Book not found");

        var boo = bookUpdateDto.MapBookUpdateDtoToBook(existingBook);
        _dbContext.Books.Update(boo);
        await _dbContext.SaveChangesAsync();

        return existingBook;
    }
    public async Task<bool> SaveChangesAsync()
    {
        return (await _dbContext.SaveChangesAsync() >= 0);
    }

    public async Task<Book> PartiallyUpdateBookAsync(Book book)
    {
        if (book == null)
        {
            throw new ArgumentNullException(nameof(book));
        }
        _dbContext.Books.Update(book);
        await SaveChangesAsync();
        return book;

    }
}

