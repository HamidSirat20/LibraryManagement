using LibraryManagement.WebAPI.Data;
using LibraryManagement.WebAPI.Helpers;
using LibraryManagement.WebAPI.Models;
using LibraryManagement.WebAPI.Models.Common;
using LibraryManagement.WebAPI.Models.Dtos;
using LibraryManagement.WebAPI.Services.Interfaces;
using LibraryManagement.WebAPI.Services.ORM.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace LibraryManagement.WebAPI.Services.Implementations;

public class BooksService : IBooksService
{
    private readonly LibraryDbContext _dbContext;
    private readonly IBooksMapper _bookMapper;
    private readonly IImageService _imageService;

    public BooksService(LibraryDbContext dbContext, IBooksMapper bookMapper, IImageService imageService)
    {
        _dbContext = dbContext ?? throw new ArgumentNullException(nameof(dbContext));
        _bookMapper = bookMapper ?? throw new ArgumentNullException(nameof(bookMapper));
        _imageService = imageService ?? throw new ArgumentNullException(nameof(imageService));
    }
    #region CreateBookAsync
    public async Task<Book> CreateBookAsync(BookDto bookCreateDto)
    {
        var book = _bookMapper.ToBook(bookCreateDto);
        if (bookCreateDto == null)
        {
            throw new ArgumentNullException(nameof(bookCreateDto));
        }
        if (bookCreateDto.File != null)
        {
            var uploadResult = await _imageService.AddImageAsync(bookCreateDto.File);
            if (uploadResult.Error != null)
            {
                throw new Exception($"Image upload failed: {uploadResult.Error.Message}");
            }
            book.CoverImageUrl = uploadResult.SecureUrl.ToString();
            book.CoverImagePublicId = uploadResult.PublicId;
        }

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
    #endregion

    #region DeleteBookByIdAsync
    public async Task<bool?> DeleteBookByIdAsync(Guid id)
    {
        var book = await _dbContext.Books.FirstOrDefaultAsync(b => b.Id == id);
        if (book == null) return false;
        _dbContext.Books.Remove(book);
        await _imageService.DeleteImageAsync(book.CoverImagePublicId!);
        return await SaveChangesAsync();
    }
    #endregion

    #region EntityExistAsync

    public async Task<bool> EntityExistsAsync(Guid id)
    {
        return await _dbContext.Books.AnyAsync(b => b.Id == id);
    }
    #endregion

    #region GetByIdAsync

    public async Task<Book?> GetByIdAsync(Guid id, bool includePublisher = false)
    {
        if (includePublisher)
        {
            return await _dbContext.Books
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
    #endregion
    #region GetAllBooksAsync
    public async Task<PaginatedResponse<Book>> ListAllBooksAsync(QueryOptions queryOptions)
    {
        if (queryOptions == null)
        {
            throw new ArgumentNullException(nameof(queryOptions));
        }
        var query = _dbContext.Books
            .Include(a => a.BookAuthors)
            .ThenInclude(ba => ba.Author)
            .Include(b => b.Publisher)
            .AsNoTracking()
            .AsQueryable();

        //filter based on genre
        if (queryOptions.Genre.HasValue)
        {
            query = query.Where(b => b.Genre == queryOptions.Genre.Value);
        }

        //search query
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

        //orderby
        if (!string.IsNullOrWhiteSpace(queryOptions.OrderBy))
        {
            query = query.ApplySorting(queryOptions.OrderBy, queryOptions.IsDescending, "Title");
        }
        else
        {

            query = query.ApplySorting(null, false, "Title");
        }
        //pagination 
        return await PaginatedResponse<Book>.CreateAsync(query, queryOptions.PageNumber, queryOptions.PageSize);

    }
    #endregion
    #region UpdateBookAsync

    public async Task<Book> UpdateBookAsync(Guid id, BookUpdateDto bookUpdateDto)
    {
        var existingBook = await _dbContext.Books
            .Include(b => b.Publisher)
            .Include(b => b.BookAuthors)
            .FirstOrDefaultAsync(b => b.Id == id);

        if (bookUpdateDto.File != null)
        {
            if (!string.IsNullOrEmpty(existingBook?.CoverImagePublicId))
            {
                await _imageService.DeleteImageAsync(existingBook.CoverImagePublicId);
            }
            var uploadResult = await _imageService.AddImageAsync(bookUpdateDto.File);
            if (uploadResult.Error != null)
            {
                throw new Exception($"Image upload failed: {uploadResult.Error.Message}");
            }
            existingBook!.CoverImageUrl = uploadResult.SecureUrl.ToString();
            existingBook.CoverImagePublicId = uploadResult.PublicId;
        }

        if (existingBook == null)
            throw new ArgumentException("Book not found");

        var bookToUpdate = _bookMapper.UpdateFromDto(existingBook, bookUpdateDto);
        await _dbContext.SaveChangesAsync();

        return existingBook;
    }
    #endregion
    #region SaveChangesAsync
    public async Task<bool> SaveChangesAsync()
    {
        return (await _dbContext.SaveChangesAsync() >= 0);
    }
    #endregion
    #region PartiallyUpdateBookAsync
    public async Task<Book> PartiallyUpdateBookAsync(Book book)
    {
        if (book == null)
        {
            throw new ArgumentNullException(nameof(book));
        }
        _dbContext.Books.Update(book);
        await _dbContext.SaveChangesAsync();
        return book;
    }
    #endregion

}

