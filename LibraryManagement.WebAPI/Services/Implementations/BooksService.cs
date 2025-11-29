using LibraryManagement.WebAPI.CustomExceptionHandler;
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
    private readonly ILogger<BooksService> _logger;
    public BooksService(LibraryDbContext dbContext, IBooksMapper bookMapper, IImageService imageService, ILogger<BooksService> logger)
    {
        _dbContext = dbContext ?? throw new ArgumentNullException(nameof(dbContext));
        _bookMapper = bookMapper ?? throw new ArgumentNullException(nameof(bookMapper));
        _imageService = imageService ?? throw new ArgumentNullException(nameof(imageService));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }
    #region CreateBookAsync
    public async Task<Book> CreateBookAsync(BookCreateDto bookCreateDto)
    {
        try
        {
            using var _ = _logger.BeginScope("Creating a new book.");

            // Validate input
            if (bookCreateDto == null)
            {
                _logger.LogError($"BookCreateDto {bookCreateDto} is null.", bookCreateDto);
                throw new ArgumentNullException(nameof(bookCreateDto));
            }

            // Map DTO to entity
            var book = _bookMapper.ToBook(bookCreateDto);

            // Handle image upload
            if (bookCreateDto.File != null)
            {
                var uploadResult = await _imageService.AddImageAsync(bookCreateDto.File);
                if (uploadResult.Error != null)
                {
                    _logger.LogError($"Image upload failed: {uploadResult.Error.Message}", uploadResult.Error.Message);
                    throw new BusinessRuleViolationException($"Image upload failed: {uploadResult.Error.Message}", "IMAAGE_ULOAD_FAILED");
                }
                book.CoverImageUrl = uploadResult.SecureUrl.ToString();
                book.CoverImagePublicId = uploadResult.PublicId;
            }

            _logger.LogDebug("Adding book to the database.");
            _dbContext.Books.Add(book);
            await SaveChangesAsync();
            return book;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "An error occurred while creating a book.");
            throw;
        }
    }
    #endregion

    #region DeleteBookByIdAsync
    public async Task<bool?> DeleteBookByIdAsync(Guid id)
    {
        if (id == Guid.Empty)
            throw new ArgumentException($"{nameof(id)} cannot be empty.");
        try
        {
            var book = await _dbContext.Books.FirstOrDefaultAsync(b => b.Id == id);
            if (book == null)
            {
                _logger.LogError($"There was not found any book associated with {id} id");
                throw new KeyNotFoundException($"Book with id {id} was not found.");
            }
            await _imageService.DeleteImageAsync(book.CoverImagePublicId!);
            _dbContext.Books.Remove(book);
            return await SaveChangesAsync();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "An error occurred while deleting a book.");
            throw;
        }
    }
    #endregion

    #region EntityExistAsync

    public async Task<bool> EntityExistsAsync(Guid id)
    {
        if (id == Guid.Empty)
            throw new ArgumentException($"{nameof(id)} cannot be empty.");
        try
        {
            return await _dbContext.Books.AnyAsync(b => b.Id == id);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "An error occurred while checking if a book exists.");
            throw;
        }
    }
    #endregion

    #region GetByIdAsync

    public async Task<Book?> GetByIdAsync(Guid id, bool includePublisher = false)
    {
        if (id == Guid.Empty)
            throw new ArgumentException($"{nameof(id)} cannot be empty.");
        try
        {
            var query = _dbContext.Books
                .Include(b => b.BookAuthors)
                .ThenInclude(ba => ba.Author)
                .AsNoTracking()
                .AsQueryable();
            if (includePublisher)
            {
                query = query.Include(b => b.Publisher);
            }
            return await query.FirstOrDefaultAsync(b => b.Id == id);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "An error occurred while retrieving a book by id.");
            throw;
        }
    }
    #endregion
    #region GetAllBooksAsync
    public async Task<PaginatedResponse<Book>> ListAllBooksAsync(QueryOptions queryOptions)
    {
        try
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
            _logger.LogDebug("Applying pagination to the book list.");
            return await PaginatedResponse<Book>.CreateAsync(query, queryOptions.PageNumber, queryOptions.PageSize);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "An error occurred while listing all books.");
            throw;
        }

    }
    #endregion
    #region UpdateBookAsync

    public async Task<Book> UpdateBookAsync(Guid id, BookUpdateDto bookUpdateDto)
    {
        try
        {
            var existingBook = await _dbContext.Books
                               .Include(b => b.Publisher)
                               .Include(b => b.BookAuthors)
                               .FirstOrDefaultAsync(b => b.Id == id);
            // Handle image update
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
            {
                _logger.LogError($"Book with id {id} not found for update.");
                throw new BusinessRuleViolationException("Book not found", "NOT_FOUND");
            }

            _logger.LogDebug($"Updating book with id {id}.");
            var bookToUpdate = _bookMapper.UpdateFromDto(existingBook, bookUpdateDto);
            await _dbContext.SaveChangesAsync();

            return existingBook;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "An error occurred while updating a book.");
            throw;
        }
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
        try
        {

            if (book == null)
            {
                _logger.LogError("Book entity to update cannot be null.");
                throw new ArgumentNullException(nameof(book));
            }
            _dbContext.Books.Update(book);
            await _dbContext.SaveChangesAsync();
            return book;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "An error occurred while partially updating a book.");
            throw;
        }
    }
    #endregion

}

