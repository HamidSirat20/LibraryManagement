using LibraryManagement.WebAPI.Models;
using LibraryManagement.WebAPI.Models.Dtos;

namespace LibraryManagement.WebAPI.Services.Interfaces
{
    public interface IUserService
    {
        Task<IEnumerable<User>> ListAllUsersAsync();
        Task<User?> GetByIdAsync(Guid id,bool includeLoansAndReservations = false);
        Task<User?> GetByEmailAsync(string email);
        Task<User?> CreateAdminAsync(UserCreateDto userCreateDto);
        Task<User?> CreateUserAsync(UserCreateDto userCreateDto);
        Task<User> UpdateUserAsync(Guid id, UserUpdateDto userUpdateDto);
        Task DeleteByIdAsync(Guid id);
        Task<bool> EntityExistAsync(Guid id);
        Task<User?> PromoteToAdminAsync(Guid memberId);
        Task<IEnumerable<User>> GetUsersWithActiveLoansAsync();
        Task<IEnumerable<User>> GetUsersWithOverdueLoansAsync();
        Task<bool> ChangePassword (Guid id, string newPassword);
    }
}
