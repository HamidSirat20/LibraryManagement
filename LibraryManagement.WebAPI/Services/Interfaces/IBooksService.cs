using LibraryManagement.WebAPI.Helpers;
using LibraryManagement.WebAPI.Models;
using LibraryManagement.WebAPI.Models.Common;
using LibraryManagement.WebAPI.Models.Dtos;

namespace LibraryManagement.WebAPI.Services.Interfaces;
public interface IBooksService
{
    Task<PaginatedResponse<Book>> ListAllBooksAsync(QueryOptions queryOption);
    Task<Book?> GetByIdAsync(Guid id, bool includePublisher = false);
    Task<Book> CreateBookAsync(BookCreateDto bookCreateDto);
    Task<Book> UpdateBookAsync(Guid id, BookUpdateDto bookUpdateDto);
    Task<Book> PartiallyUpdateBookAsync(Book book);
    Task<bool> EntityExistsAsync(Guid id);
    Task<bool?> DeleteBookByIdAsync(Guid id);
    Task<bool> SaveChangesAsync();
}
