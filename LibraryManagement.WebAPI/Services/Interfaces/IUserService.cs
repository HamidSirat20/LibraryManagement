using LibraryManagement.WebAPI.Models;
using LibraryManagement.WebAPI.Models.Dtos;

namespace LibraryManagement.WebAPI.Services.Interfaces
{
    public interface IUserService
    {
        Task<IEnumerable<UserReadDto>> ListAllUsersAsync();
        Task<UserReadDto?> GetByIdAsync(Guid id,bool includeLoansAndReservations = false);
        Task<UserReadDto?> GetByEmailAsync(string email);
        Task<UserReadDto?> CreateAdminAsync(UserCreateDto userCreateDto);
        Task<UserReadDto?> CreateUserAsync(UserCreateDto userCreateDto);
        Task<UserReadDto> UpdateUserAsync(Guid id, UserUpdateDto userUpdateDto);
        Task DeleteByIdAsync(Guid id);
        Task<bool> EntityExistAsync(Guid id);
        Task<UserReadDto?> PromoteToAdminAsync(Guid memberId);
        Task<IEnumerable<User>> GetUsersWithActiveLoansAsync();
        Task<IEnumerable<User>> GetUsersWithOverdueLoansAsync();
        Task<bool> ChangePassword (Guid id, string newPassword);
    }
}
