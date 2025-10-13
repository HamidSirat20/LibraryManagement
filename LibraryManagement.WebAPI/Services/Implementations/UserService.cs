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
        public Task<bool> ChangePassword(ChangePasswordDto changePasswordDto)
        {
            throw new NotImplementedException();
        }

        public async Task<UserReadDto> CreateAdminAsync(UserCreateDto userCreateDto)
        {
            var user = userCreateDto.MapUserCreateDtoToUser();
            user.Role = UserRole.Admin;
            await _dbContext.Users.AddAsync(user);
            _dbContext.SaveChanges();
            return user.MapUserToUserReadDto();
        }

        public async Task<UserReadDto?> CreateUserAsync(UserCreateDto userCreateDto)
        {
            var user = userCreateDto.MapUserCreateDtoToUser();
            user.Role = UserRole.User;
            await _dbContext.Users.AddAsync(user);
            _dbContext.SaveChanges();
            return user.MapUserToUserReadDto();
        }

        public async Task DeleteByIdAsync(Guid id)
        {
           var user = await _dbContext.Users.FindAsync(id);
            if(user == null)
            {
                throw new ArgumentNullException(nameof(user), "User not found");
            }
            _dbContext.Users.Remove(user);
            await _dbContext.SaveChangesAsync();
        }

        public async Task<bool> EntityExistAsync(Guid id)
        {
           return await _dbContext.Users.AnyAsync(x => x.Id == id);
        }

        public async Task<UserReadDto?> GetByEmailAsync(string email)
        {
            var user  = await _dbContext.Users.FirstOrDefaultAsync(x => x.Email == email);
            if (user == null)
            {
                return null;
            }
            return user.MapUserToUserReadDto();
        }

        public async Task<UserReadDto?> GetByIdAsync(Guid id, bool includeLoansAndReservations = false)
        {

            if (id == Guid.Empty)
            {
                throw new ArgumentNullException("User ID cannot be empty", nameof(id));
            }

            try
            {
                IQueryable<User> query = _dbContext.Users;

                if (includeLoansAndReservations)
                {
                    query = query
                        .Include(x => x.Reservations)
                        .Include(x => x.Loans)
                        .Include(x => x.LateReturnOrLostFees);
                }

                var user = await query
                    .AsNoTracking() 
                    .FirstOrDefaultAsync(x => x.Id == id);

                return user?.MapUserToUserReadDto();
            }
            catch (Exception)
            {
               
                throw new ArgumentNullException("An error occurred while retrieving the user.");
            }
        }

        public async Task<IEnumerable<User>> GetUsersWithActiveLoansAsync()
        {
            var user  = await _dbContext.Users
                .Include(u => u.Loans)
                .Where(u => u.Loans.Any(l => l.LoanStatus == LoanStatus.Active))
                .ToListAsync();
            return user;
        }

        public async Task<IEnumerable<User>> GetUsersWithOverdueLoansAsync()
        {
            var users = await _dbContext.Users
                .Include(u => u.Loans)
                .Where(u => u.Loans.Any(l => l.LoanStatus == LoanStatus.Overdue))
                .ToListAsync();
            return users;
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

        public async Task<UserReadDto?> PromoteToAdminAsync(Guid memberId)
        {
            var user = await _dbContext.Users.FirstOrDefaultAsync(x => x.Id == memberId);
            if(user == null)
            {
                return null;
            }
            user.Role = UserRole.Admin;
            await _dbContext.SaveChangesAsync();
            return user.MapUserToUserReadDto();
        }

        public async Task<UserReadDto> UpdateUserAsync(Guid id, UserUpdateDto userUpdateDto)
        {
            var userToUpdate =await _dbContext.Users.FirstOrDefaultAsync(x => x.Id ==id);
            if(userToUpdate == null)
            {
                throw new ArgumentNullException(nameof(userToUpdate), "User not found");
            }
             userUpdateDto.MapUserUpdateDtoToUser(userToUpdate);
            await _dbContext.SaveChangesAsync();
            return userToUpdate.MapUserToUserReadDto();
        }
    }
}
