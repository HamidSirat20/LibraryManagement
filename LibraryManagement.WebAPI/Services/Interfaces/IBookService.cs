using LibraryManagement.WebAPI.Models;
using LibraryManagement.WebAPI.Models.Dtos;

namespace LibraryManagement.WebAPI.Services.Interfaces;
    public interface IBookService
    {
        Task<IEnumerable<BookReadDto>> ListAllAsync();
        Task<BookReadDto?> GetByIdAsync(Guid id);
        Task<BookReadDto?> GetByIdWithAuthorsAsync(Guid id);
        Task<BookReadDto> CreateEntityAsync(Book book);
        Task<BookReadDto> UpdateEntityAsync(Book book);
        Task<bool> EntityExistAsync(Book book);
        Task<bool?> DeleteByIdAsync(Guid id);
    }
