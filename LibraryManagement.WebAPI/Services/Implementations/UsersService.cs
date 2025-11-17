using LibraryManagement.WebAPI.Data;
using LibraryManagement.WebAPI.Models;
using LibraryManagement.WebAPI.Models.Dtos;
using LibraryManagement.WebAPI.Services.Interfaces;
using Microsoft.EntityFrameworkCore;
using LibraryManagement.WebAPI.Services.ORM.Interfaces;

namespace LibraryManagement.WebAPI.Services.Implementations;
public class UsersService : IUsersService
{
    private readonly LibraryDbContext _dbContext;
    private readonly IUsersMapper _userMapper;
    private readonly IPasswordService _passwordService;
    private readonly IImageService _imageService;

    public UsersService(LibraryDbContext libraryDbContext, IUsersMapper userMapper, IPasswordService passwordService, IImageService imageService)
    {
        _dbContext = libraryDbContext ?? throw new ArgumentNullException(nameof(libraryDbContext));
        _userMapper = userMapper ?? throw new ArgumentNullException(nameof(userMapper));
        _passwordService = passwordService ?? throw new ArgumentNullException(nameof(passwordService));
        _imageService = imageService ?? throw new ArgumentNullException(nameof(imageService));
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

    public async Task<User> CreateAdminAsync(UserCreateDto userCreateDto)
    {
        var user = _userMapper.ToEntity(userCreateDto);
        if (userCreateDto.File != null)
        {
            var uploadResult = await _imageService.AddImageAsync(userCreateDto.File);
            if (uploadResult.Error != null)
            {
                throw new Exception($"Image upload failed: {uploadResult.Error.Message}");
            }
            user.AvatarUrl = uploadResult.SecureUrl.ToString();
            user.PublicId = uploadResult.PublicId;
        }
        //hash password
        _passwordService.HashPassword(userCreateDto.Password, out var hashedPassword, out var salt);
        user.Password = hashedPassword;
        user.Salt = salt;

        user.Role = UserRole.Admin;
        await _dbContext.Users.AddAsync(user);
        _dbContext.SaveChanges();
        return user;
    }

    public async Task<User?> CreateUserAsync(UserCreateDto userCreateDto)
    {
        var user = _userMapper.ToEntity(userCreateDto);
        if (userCreateDto.File != null)
        {
            var uploadResult = await _imageService.AddImageAsync(userCreateDto.File);
            if (uploadResult.Error != null)
            {
                throw new Exception($"Image upload failed: {uploadResult.Error.Message}");
            }
            user.AvatarUrl = uploadResult.SecureUrl.ToString();
            user.PublicId = uploadResult.PublicId;
        }
        //hash password
        _passwordService.HashPassword(userCreateDto.Password, out var hashedPassword, out var salt);
        user.Password = hashedPassword;
        user.Salt = salt;
        user.Role = UserRole.User;

        await _dbContext.Users.AddAsync(user);
        _dbContext.SaveChanges();
        return user;
    }

    public async Task DeleteByIdAsync(Guid id)
    {
        var user = await _dbContext.Users.FindAsync(id);
        if (user == null)
        {
            throw new ArgumentNullException(nameof(user), "User not found");
        }
        _dbContext.Users.Remove(user);
        await _imageService.DeleteImageAsync(user.PublicId!);
        await _dbContext.SaveChangesAsync();
    }

    public async Task<bool> EntityExistAsync(Guid id)
    {
        return await _dbContext.Users.AnyAsync(x => x.Id == id);
    }

    public async Task<User?> GetByEmailAsync(string email)
    {
        var user = await _dbContext.Users.FirstOrDefaultAsync(x => x.Email == email);
        if (user == null)
        {
            return null;
        }
        return user;
    }

    public async Task<User?> GetByIdAsync(Guid id, bool includeLoansAndReservations = false)
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

            return user;
        }
        catch (Exception)
        {

            throw new ArgumentNullException("An error occurred while retrieving the user.");
        }
    }

    public async Task<IEnumerable<User>> GetUsersWithActiveLoansAsync()
    {
        var user = await _dbContext.Users
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
    public async Task<IEnumerable<User>> ListAllUsersAsync()
    {
        var users = await _dbContext.Users.ToListAsync();

        return users;
    }

    public async Task<User?> PromoteToAdminAsync(Guid memberId)
    {
        var user = await _dbContext.Users.FirstOrDefaultAsync(x => x.Id == memberId);
        if (user == null)
        {
            return null;
        }
        user.Role = UserRole.Admin;
        await _dbContext.SaveChangesAsync();
        return user;
    }

    public async Task<User> UpdateUserAsync(Guid id, UserUpdateDto userUpdateDto)
    {
        var user = await _dbContext.Users.FirstOrDefaultAsync(x => x.Id == id);
        if (user == null)
        {
            throw new ArgumentNullException(nameof(user), "User not found");
        }
        if (userUpdateDto.File != null)
        {
            if (!string.IsNullOrEmpty(user.PublicId))
            {
                await _imageService.DeleteImageAsync(user.PublicId);
            }
            var uploadResult = await _imageService.AddImageAsync(userUpdateDto.File);
            if (uploadResult.Error != null)
            {
                throw new Exception($"Image upload failed: {uploadResult.Error.Message}");
            }
            user.AvatarUrl = uploadResult.SecureUrl.ToString();
            user.PublicId = uploadResult.PublicId;
        }
        _userMapper.UpdateFromDto(user, userUpdateDto);
        await _dbContext.SaveChangesAsync();
        return user;
    }
}
