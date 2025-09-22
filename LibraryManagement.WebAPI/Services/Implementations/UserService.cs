using LibraryManagement.WebAPI.Data;
using LibraryManagement.WebAPI.Models;
using LibraryManagement.WebAPI.Models.Dtos;
using LibraryManagement.WebAPI.Services.Interfaces;
using Microsoft.EntityFrameworkCore;
using LibraryManagement.WebAPI.Services.ORM; 

namespace LibraryManagement.WebAPI.Services.Implementations
{
    public class UserService : IUserService
    {
        private readonly LibraryDbContext _dbContext;

        public UserService(LibraryDbContext libraryDbContext)
        {
            _dbContext = libraryDbContext ?? throw new ArgumentNullException(nameof(libraryDbContext));
        }
        public Task<UserReadDto?> CreateAdminAsync(UserCreateDto userCreateDto)
        {
            throw new NotImplementedException();
        }

        public Task<UserReadDto?> CreateUserAsync(UserCreateDto userCreateDto)
        {
            throw new NotImplementedException();
        }

        public Task DeleteByIdAsync(Guid id)
        {
            throw new NotImplementedException();
        }

        public Task<bool> EntityExistAsync(Guid id)
        {
            throw new NotImplementedException();
        }

        public Task<UserReadDto?> GetByEmailAsync(string email)
        {
            throw new NotImplementedException();
        }

        public Task<UserReadDto?> GetByIdAsync(Guid id)
        {
            throw new NotImplementedException();
        }

        public Task<IEnumerable<User>> GetUsersWithActiveLoansAsync()
        {
            throw new NotImplementedException();
        }

        public Task<IEnumerable<User>> GetUsersWithOverdueLoansAsync()
        {
            throw new NotImplementedException();
        }

        public async Task<IEnumerable<UserReadDto>> ListAllUsersAsync()
        {
           var users = await _dbContext.Users.ToListAsync();
            var userReadDto = new List<UserReadDto>();
            foreach (var user in users)
            {
                userReadDto.Add(user.MapUserToUserReadDto());
            }
            return userReadDto;
        }

        public Task<UserReadDto?> PromoteToAdminAsync(Guid memberId)
        {
            throw new NotImplementedException();
        }

        public Task<UserReadDto> UpdateUserAsync(UserUpdateDto userUpdateDto)
        {
            throw new NotImplementedException();
        }
    }
}
