using LibraryManagement.WebAPI.Data;
using LibraryManagement.WebAPI.Models;
using LibraryManagement.WebAPI.Models.Common;
using LibraryManagement.WebAPI.Models.Dtos;
using LibraryManagement.WebAPI.Services.Interfaces;
using LibraryManagement.WebAPI.Services.ORM;
using Microsoft.EntityFrameworkCore;

namespace LibraryManagement.WebAPI.Services.Implementations;

public class BookService : IBookService
{
    private readonly LibraryDbContext _dbContext;
    const int MaxPageSize = 20;

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

    public async Task<IEnumerable<Book>> ListAllBooksAsync(QueryOptions queryOptions)
    {
        var query = _dbContext.Books
            .Include(a => a.BookAuthors)
            .ThenInclude(ba => ba.Author)
            .Include(b => b.Publisher)
            .AsNoTracking()
            .AsQueryable();

        if (!string.IsNullOrWhiteSpace(queryOptions.SearchTerm))
        {
            var searchTerm = queryOptions.SearchTerm.Trim().ToLower();
            query = query.Where(b => b.Title.ToLower().Contains(searchTerm) ||
                                     b.Description.ToLower().Contains(searchTerm) ||
                                     b.Genre.ToString().ToLower().Contains(searchTerm) ||
                                     b.Publisher.Name.ToLower().Contains(searchTerm) ||
                                     b.BookAuthors.Any(ba => ba.Author.FirstName.ToLower().Contains(searchTerm) ||
                                                            ba.Author.LastName.ToLower().Contains(searchTerm)));
        }
        if (!string.IsNullOrWhiteSpace(queryOptions.SortBy))
        {
            var sortBy = queryOptions.SortBy.Trim().ToLower();
            var isDescending = queryOptions.IsDescending;
            var allowedSortFields = new[] { "title", "publisheddate", "genre", "publisher","author" };

            if (!allowedSortFields.Contains(sortBy))
            {
                sortBy = "title";
            }

            query = (sortBy, isDescending) switch
            {
                ("title", false) => query.OrderBy(b => b.Title),
                ("title", true) => query.OrderByDescending(b => b.Title),
                ("publisheddate", false) => query.OrderBy(b => b.PublishedDate),
                ("publisheddate", true) => query.OrderByDescending(b => b.PublishedDate),
                ("genre", false) => query.OrderBy(b => b.Genre),
                ("genre", true) => query.OrderByDescending(b => b.Genre),
                ("publisher", false) => query.OrderBy(b => b.Publisher != null ? b.Publisher.Name : ""),
                ("publisher", true) => query.OrderByDescending(b => b.Publisher != null ? b.Publisher.Name : ""),
                _ => query.OrderBy(b => b.Title)
            };

        }
        return await query.Skip(queryOptions.PageSize * (queryOptions.PageNumber - 1)).Take(queryOptions.PageSize).ToListAsync();
        
    }

    public async Task<Book> UpdateBookAsync(Guid id, BookUpdateDto bookUpdateDto)
    {
        var existingBook = await _dbContext.Books
            .Include(b => b.Publisher)
            .Include(b => b.BookAuthors)
            .FirstOrDefaultAsync(b => b.Id == id);

        if (existingBook == null)
            throw new ArgumentException("Book not found");

        var bookToUpdate = bookUpdateDto.MapBookUpdateDtoToBook(existingBook);
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
        await SaveChangesAsync();
        return book;
    }
}

