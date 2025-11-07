using LibraryManagement.WebAPI.Helpers;
using LibraryManagement.WebAPI.Models;
using LibraryManagement.WebAPI.Models.Common;
using LibraryManagement.WebAPI.Models.Dtos;

namespace LibraryManagement.WebAPI.Services.Interfaces;
    public interface IAuthorService
    {
        Task<PaginatedResponse<AuthorReadDto>> ListAllAuthorAsync(QueryOptions queryOptions);
        Task<PaginatedResponse<AuthorWithBooksDto>> ListAllAuthorsWithBookAsync(QueryOptions queryOptions);
        Task<Author?> GetAuthorByIdAsync(Guid id);
        Task<Author?> GetAuthorByEmailAsync(string email);
        Task<Author?> CreateAuthorAsync(AuthorCreateDto authorCreateDto);
        Task<Author> UpdateAuthorAsync(Guid id, AuthorUpdateDto authorUpdateDto);
        Task<Author> DeleteAuthorAsync(Guid id);
        Task<bool> EntityExistAsync(Guid id);
    }

