using LibraryManagement.WebAPI.Models;
using LibraryManagement.WebAPI.Models.Dtos;

namespace LibraryManagement.WebAPI.Services.Interfaces;
    public interface IBookService
    {
        Task<IEnumerable<Book>> ListAllBooksAsync();
        Task<Book?> GetByIdAsync(Guid id,bool includePublisher = false);
        Task<Book> CreateEntityAsync(Book book);
        Task<Book> UpdateEntityAsync(Book book);
        Task<bool> EntityExistAsync(Book book);
        Task<bool?> DeleteByIdAsync(Guid id);
    }
