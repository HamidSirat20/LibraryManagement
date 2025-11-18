using LibraryManagement.WebAPI.Data;
using LibraryManagement.WebAPI.Models;
using LibraryManagement.WebAPI.Models.Dtos;
using LibraryManagement.WebAPI.Services.Interfaces;
using LibraryManagement.WebAPI.Services.ORM.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace LibraryManagement.WebAPI.Services.Implementations;
public class BookCollectionsService : IBookCollectionsService
{
    private readonly LibraryDbContext _dbContext;
    private readonly IBooksMapper _bookMapper;

    public BookCollectionsService(LibraryDbContext libraryDb, IBooksMapper bookMapper)
    {
        _dbContext = libraryDb ?? throw new ArgumentNullException(nameof(libraryDb));
        _bookMapper = bookMapper;
    }

    public async Task<IEnumerable<BookReadDto>> GetBookCollectionsAsync(IEnumerable<Guid> bookIds)
    {
        if (bookIds == null || !bookIds.Any())
        {
            throw new ArgumentException(nameof(bookIds));
        }

        var books = await _dbContext.Books
            .Where(b => bookIds.Contains(b.Id))
            .AsNoTracking()
            .OrderByDescending(b => b.PublishedDate)
            .ToListAsync();

        if (books.Count != bookIds.Count())
        {
            throw new InvalidOperationException("One or more book IDs were not found.");
        }
        return books.Select(book => _bookMapper.ToBookReadDto(book));
    }

    public async Task<IEnumerable<BookReadDto>> CreateBooksAsync(IEnumerable<BookDto> bookCreateDtos)
    {
        if (bookCreateDtos == null || !bookCreateDtos.Any())
        {
            throw new ArgumentNullException(nameof(bookCreateDtos));
        }

        var books = new List<Book>();

        foreach (var bookCreateDto in bookCreateDtos)
        {
            var book = new Book()
            {
                Title = bookCreateDto.Title,
                Description = bookCreateDto.Description,
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
                    Book = book
                });
            }

            books.Add(book);
            _dbContext.Books.Add(book);
        }

        await _dbContext.SaveChangesAsync();
        return books.Select(book => _bookMapper.ToBookReadDto(book));
    }
}

