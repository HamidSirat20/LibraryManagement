using LibraryManagement.WebAPI.Models;
using LibraryManagement.WebAPI.Models.Common;
using LibraryManagement.WebAPI.Models.Dtos;

namespace LibraryManagement.WebAPI.Services.Interfaces;
    public interface IBookService
    {
        Task<(IEnumerable<Book>,PaginationMetadata)> ListAllBooksAsync(QueryOptions queryOption);
        Task<Book?> GetByIdAsync(Guid id,bool includePublisher = false);
        Task<Book> CreateBookAsync(BookCreateDto bookCreateDto);
        Task<Book> UpdateBookAsync(Guid id, BookUpdateDto bookUpdateDto);
        Task<Book> PartiallyUpdateBookAsync(Book book);
        Task<bool> EntityExistAsync(Guid id);
        Task<bool?> DeleteBookByIdAsync(Guid id);
        Task<bool> SaveChangesAsync ();
}
