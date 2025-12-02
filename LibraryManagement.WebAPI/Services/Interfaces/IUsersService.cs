using LibraryManagement.WebAPI.Helpers;
using LibraryManagement.WebAPI.Models;
using LibraryManagement.WebAPI.Models.Common;
using LibraryManagement.WebAPI.Models.Dtos;

namespace LibraryManagement.WebAPI.Services.Interfaces;
public interface IUsersService
{
    Task<PaginatedResponse<User>> ListAllUsersAsync( QueryOptions queryOptions);
    Task<User?> GetByIdAsync(Guid id, bool includeLoansAndReservations = false);
    Task<User?> GetByEmailAsync(string email);
    Task<User?> CreateAdminAsync(UserCreateDto userCreateDto);
    Task<User?> CreateUserAsync(UserCreateDto userCreateDto);
    Task<User> UpdateUserAsync(Guid id, UserUpdateDto userUpdateDto);
    Task DeleteByIdAsync(Guid id);
    Task<bool> EntityExistAsync(Guid id);
    Task<User?> PromoteToAdminAsync(Guid memberId);
    Task<User> ExtendUserMembership(Guid memberId);
    Task<IEnumerable<User>> GetUsersWithActiveLoansAsync();
    Task<IEnumerable<User>> GetUsersWithOverdueLoansAsync();
    Task<bool> ChangePassword(Guid id, string newPassword);
}

