using LibraryManagement.WebAPI.Models;
using LibraryManagement.WebAPI.Models.Dtos;

namespace LibraryManagement.WebAPI.Services.Interfaces
{
    public interface IUserService
    {
        Task<IEnumerable<UserReadDto>> ListAllUsersAsync();
        Task<UserReadDto?> GetByIdAsync(Guid id);
        Task<UserReadDto?> GetByEmailAsync(string email);
        Task<UserReadDto?> CreateAdminAsync(UserCreateDto userCreateDto);
        Task<UserReadDto?> CreateUserAsync(UserCreateDto userCreateDto);
        Task<UserReadDto> UpdateUserAsync(UserUpdateDto userUpdateDto);
        Task DeleteByIdAsync(Guid id);
        Task<bool> EntityExistAsync(Guid id);
        Task<UserReadDto?> PromoteToAdminAsync(Guid memberId);
        Task<IEnumerable<User>> GetUsersWithActiveLoansAsync();
        Task<IEnumerable<User>> GetUsersWithOverdueLoansAsync();
    }
}
