using LibraryManagement.WebAPI.CustomExceptionHandler;
using LibraryManagement.WebAPI.Data;
using LibraryManagement.WebAPI.Helpers;
using LibraryManagement.WebAPI.Models;
using LibraryManagement.WebAPI.Models.Common;
using LibraryManagement.WebAPI.Models.Dtos;
using LibraryManagement.WebAPI.Services.Interfaces;
using LibraryManagement.WebAPI.Services.ORM.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace LibraryManagement.WebAPI.Services.Implementations;
public class UsersService : IUsersService
{
    private readonly LibraryDbContext _context;
    private readonly IUsersMapper _userMapper;
    private readonly IPasswordService _passwordService;
    private readonly IImageService _imageService;
    private readonly ILogger<UsersService> _logger;

    public UsersService(LibraryDbContext libraryDbContext, IUsersMapper userMapper, IPasswordService passwordService, IImageService imageService, ILogger<UsersService> logger)
    {
        _context = libraryDbContext ?? throw new ArgumentNullException(nameof(libraryDbContext));
        _userMapper = userMapper ?? throw new ArgumentNullException(nameof(userMapper));
        _passwordService = passwordService ?? throw new ArgumentNullException(nameof(passwordService));
        _imageService = imageService ?? throw new ArgumentNullException(nameof(imageService));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }
    public async Task<bool> ChangePassword(Guid id, string newPassword)
    {
        try
        {
            var foundUser = await _context.Users.FindAsync(id);
            if (foundUser == null)
            {
                _logger.LogWarning("User with ID {UserId} not found while attempting to change password", id);
                throw new BusinessRuleViolationException("User not found", "NOT_FOUND");
            }
            _passwordService.HashPassword(newPassword, out var hashedPassword, out var salt);
            foundUser.Password = hashedPassword;
            foundUser.Salt = salt;
            _logger.LogDebug("Changing password for user with ID {UserId}", id);
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "An error occurred while changing the password for user with ID {UserId}", id);
            throw;
        }
    }

    public async Task<User> CreateAdminAsync(UserCreateDto userCreateDto)
    {
        if (userCreateDto == null)
            throw new ArgumentNullException(nameof(userCreateDto), "User creation data cannot be null");

        try
        {
            _logger.LogInformation("Creating admin user with email {Email}", userCreateDto.Email);

            //trim inputs for email and password
            userCreateDto.Email = userCreateDto.Email?.Trim();
            userCreateDto.Password = userCreateDto.Password?.Trim();

            var user = _userMapper.ToEntity(userCreateDto);
            //upload avatar if exists
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
            await _context.Users.AddAsync(user);
            _context.SaveChanges();
            _logger.LogDebug("Admin user with email {Email} created successfully", userCreateDto.Email);
            return user;
        }
        catch (DbUpdateException dbEx)
        {
            _logger.LogError(dbEx, "Database update error while creating an admin user with email {Email}", userCreateDto.Email);
            throw new BusinessRuleViolationException("A database error occurred while creating the admin user.", "DB_UPDATE_ERROR");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "An error occurred while creating an admin user with email {Email}", userCreateDto.Email);
            throw;
        }
    }

    public async Task<User?> CreateUserAsync(UserCreateDto userCreateDto)
    {
        if (userCreateDto == null)
            throw new ArgumentNullException(nameof(userCreateDto), "User creation data cannot be null");
        try
        {
            _logger.LogInformation("Creating user with email {Email}", userCreateDto.Email);
            //trim inputs for email and password
            userCreateDto.Email = userCreateDto.Email?.Trim();
            userCreateDto.Password = userCreateDto.Password?.Trim();

            var user = _userMapper.ToEntity(userCreateDto);
            if (userCreateDto.File == null)
                    throw new BusinessRuleViolationException("User avatar is required.", "AVATAR_REQUIRED");
            // upload avatar if exists
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

            await _context.Users.AddAsync(user);
            _context.SaveChanges();
            _logger.LogDebug("User with email {Email} created successfully", userCreateDto.Email);
            return user;
        }
        catch (DbUpdateException dbEx)
        {
            _logger.LogError(dbEx, "Database update error while creating a user with email {Email}", userCreateDto.Email);
            throw new BusinessRuleViolationException("A database error occurred while creating the user.", "DB_UPDATE_ERROR");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "An error occurred while creating a user with email {Email}", userCreateDto.Email);
            throw;
        }
    }

    public async Task DeleteByIdAsync(Guid id)
    {
        if (id == Guid.Empty)
            throw new ArgumentException("User ID cannot be empty", nameof(id));
        try
        {
            var user = await _context.Users.FindAsync(id);
            if (user == null)
            {
                _logger.LogWarning("User with ID {UserId} not found while attempting to delete", id);
                throw new KeyNotFoundException($"User not found with {id} id.");
            }
            await _imageService.DeleteImageAsync(user.PublicId!);
            _context.Users.Remove(user);
            await _context.SaveChangesAsync();
            _logger.LogInformation("User with ID {UserId} deleted successfully", id);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "An error occurred while deleting the user with ID {UserId}", id);
            throw;
        }
    }

    public async Task<bool> EntityExistAsync(Guid id)
    {
        return await _context.Users.AnyAsync(x => x.Id == id);
    }

    public async Task<User?> GetByEmailAsync(string email)
    {
        if (string.IsNullOrWhiteSpace(email))
            throw new ArgumentException("Email cannot be null or empty", nameof(email));
        try
        {
            var user = await _context.Users.FirstOrDefaultAsync(x => x.Email == email);
       
            return user;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"There was an error while retrieve user with {email}.");
            throw;
        }
    }

    public async Task<User?> GetByIdAsync(Guid id, bool includeLoansAndReservations = false)
    {

        if (id == Guid.Empty)
            throw new ArgumentException("User ID cannot be empty", nameof(id));

        try
        {
            var query = _context.Users
                .AsNoTracking()
                .AsQueryable();

            if (includeLoansAndReservations)
            {
                query = query
                    .Include(x => x.Reservations)
                    .Include(x => x.Loans)
                    .Include(x => x.LateReturnOrLostFees);
            }

            return await query.FirstOrDefaultAsync(u => u.Id == id);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"An error occurred while retrieving user with {id} id.");
            throw;
        }
    }

    public async Task<IEnumerable<User>> GetUsersWithActiveLoansAsync()
    {
        try
        {
            var user = await _context.Users
                            .Include(u => u.Loans)
                            .Where(u => u.Loans.Any(l => l.LoanStatus == LoanStatus.Active))
                            .ToListAsync();
            if (user == null)
            {
                _logger.LogWarning("No user with active loan found.");
                throw new BusinessRuleViolationException("No user with active loan found.");
            }
            return user;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "An error occurred while retrieveing users.");
            throw;
        }
    }

    public async Task<IEnumerable<User>> GetUsersWithOverdueLoansAsync()
    {
        try
        {
            var users = await _context.Users
                              .Include(u => u.Loans)
                              .Where(u => u.Loans.Any(l => l.LoanStatus == LoanStatus.Overdue))
                              .ToListAsync();
            if (users == null)
            {
                _logger.LogWarning("");
                throw new BusinessRuleViolationException("An error occurred while retrieving users.");
            }
            return users;
        }
        catch (Exception ex)
        {
            _logger.LogError("No user with active loan found.");
            throw;
        }

    }
    public async Task<PaginatedResponse<User>> ListAllUsersAsync(QueryOptions queryOptions)
    {
        try
        {
            var query = _context.Users
                            .AsNoTracking()
                            .AsQueryable();

            // Apply sort
            if (!string.IsNullOrWhiteSpace(queryOptions.SearchTerm))
            {
                var sortBy = queryOptions.OrderBy.Trim().ToLower();
                query = query.ApplySorting(sortBy, queryOptions.IsDescending, "LastName");

            }
            //apply search
            if (!string.IsNullOrWhiteSpace(queryOptions.SearchTerm))
            {
                var searchTerm = queryOptions.SearchTerm.Trim().ToLower();
                query = query.Where(l => l.Address.ToLower().Contains(searchTerm) ||
                                         l.FirstName.ToLower().Contains(searchTerm) ||
                                         l.LastName.ToLower().Contains(searchTerm) ||
                                         l.Email.ToString().ToLower().Contains(searchTerm));
            }

            var paginatedLoans = await PaginatedResponse<User>.CreateAsync(query, queryOptions.PageNumber, queryOptions.PageSize);
            return paginatedLoans;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "An error occurred while retrieving all users.");
            throw;
        }
    }

    public async Task<User?> PromoteToAdminAsync(Guid memberId)
    {
        if (memberId == Guid.Empty)
            throw new ArgumentException("User id should not be empty.", nameof(memberId));
        try
        {
            var user = await _context.Users.FirstOrDefaultAsync(x => x.Id == memberId);
            if (user == null)
            {
                _logger.LogWarning("User with {memberId} was not found.", memberId);
                throw new BusinessRuleViolationException("User with {memberId} was not found.", "NOT_FOUND");
            }
            user.Role = UserRole.Admin;
            await _context.SaveChangesAsync();
            return user;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "An error occurred while promoting user to admin.");
            throw;
        }
    }

    public async Task<User> UpdateUserAsync(Guid id, UserUpdateDto userUpdateDto)
    {
        if (id == Guid.Empty || userUpdateDto == null)
            throw new ArgumentException("Id should not be empty or userDto should not be null.", nameof(id));

        try
        {
            var user = await _context.Users.FirstOrDefaultAsync(x => x.Id == id);
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
            await _context.SaveChangesAsync();
            return user;
        }
        catch (DbUpdateException ex)
        {
            _logger.LogError(ex, $"An error occurred in DB while updating user.");
            throw;
        }
        catch (Exception ex)
        {

            _logger.LogError(ex,$"An error occurred while updating user with {id}.", id);
            throw;
        }
    }
}
