using LibraryManagement.WebAPI.Models;
using LibraryManagement.WebAPI.Models.Dtos;

namespace LibraryManagement.WebAPI.Services.Interfaces;
    public interface IBookService
    {
        Task<IEnumerable<Book>> ListAllBooksAsync();
        Task<Book?> GetByIdAsync(Guid id,bool includePublisher = false);
        Task<Book> CreateBookAsync(BookCreateDto bookCreateDto);
        Task<Book> UpdateBookAsync(Book book);
        Task<bool> EntityExistAsync(Guid id);
        Task<bool?> DeleteByIdAsync(Guid id);
        Task<bool> SaveChangesAsync ();
}
