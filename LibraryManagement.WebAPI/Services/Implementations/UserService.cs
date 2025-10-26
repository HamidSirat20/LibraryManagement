using LibraryManagement.WebAPI.Data;
using LibraryManagement.WebAPI.Models;
using LibraryManagement.WebAPI.Models.Dtos;
using LibraryManagement.WebAPI.Services.Interfaces;
using Microsoft.EntityFrameworkCore;
using LibraryManagement.WebAPI.Services.ORM.Interfaces;

namespace LibraryManagement.WebAPI.Services.Implementations
{
    public class UserService : IUserService
    {
        private readonly LibraryDbContext _dbContext;
        private readonly IUserMapper _userMapper;
        private readonly IPasswordService _passwordService;

        public UserService(LibraryDbContext libraryDbContext, IUserMapper userMapper,IPasswordService passwordService)
        {
            _dbContext = libraryDbContext ?? throw new ArgumentNullException(nameof(libraryDbContext));
            _userMapper = userMapper;
            _passwordService = passwordService ?? throw new ArgumentNullException(nameof(passwordService));
        }
        public async Task<bool> ChangePassword(Guid id, string newPassword)
        {
            var foundUser = await _dbContext.Users.FindAsync(id);
            if (foundUser == null)
            {
                throw new Exception("User not found");
            }
            _passwordService.HashPassword(newPassword, out var hashedPassword, out var salt);
            foundUser.Password = hashedPassword;
            foundUser.Salt = salt;
            return true;
        }

        public async Task<UserReadDto> CreateAdminAsync(UserCreateDto userCreateDto)
        {
            var user = _userMapper.ToEntity(userCreateDto);

            //hash password
            _passwordService.HashPassword(userCreateDto.Password, out var hashedPassword, out var salt);
            user.Password = hashedPassword;
            user.Salt = salt;

            user.Role = UserRole.Admin;
            await _dbContext.Users.AddAsync(user);
            _dbContext.SaveChanges();
            return _userMapper.ToReadDto(user);
        }

        public async Task<UserReadDto?> CreateUserAsync(UserCreateDto userCreateDto)
        {
            var user = _userMapper.ToEntity(userCreateDto);
            //hash password
            _passwordService.HashPassword(userCreateDto.Password, out var hashedPassword, out var salt);
            user.Password = hashedPassword;
            user.Salt = salt;
            user.Role = UserRole.User;

            await _dbContext.Users.AddAsync(user);
            _dbContext.SaveChanges();
            return _userMapper.ToReadDto(user);
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
            return _userMapper.ToReadDto(user);
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

                return _userMapper.ToReadDto(user);
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
                userReadDto.Add(_userMapper.ToReadDto(user));
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
            return _userMapper.ToReadDto(user);
        }

        public async Task<UserReadDto> UpdateUserAsync(Guid id, UserUpdateDto userUpdateDto)
        {
            var user =await _dbContext.Users.FirstOrDefaultAsync(x => x.Id ==id);
            if(user == null)
            {
                throw new ArgumentNullException(nameof(user), "User not found");
            }
             _userMapper.UpdateFromDto(user, userUpdateDto);
            await _dbContext.SaveChangesAsync();
            return _userMapper.ToReadDto(user);
        }
    }
}
