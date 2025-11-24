using CloudinaryDotNet.Actions;
using LibraryManagement.Test.Fixtures;
using LibraryManagement.Test.Test_Data_Builders;
using LibraryManagement.WebAPI.CustomExceptionHandler;
using LibraryManagement.WebAPI.Data;
using LibraryManagement.WebAPI.Models;
using LibraryManagement.WebAPI.Models.Common;
using LibraryManagement.WebAPI.Models.Dtos;
using LibraryManagement.WebAPI.Services.Implementations;
using Microsoft.AspNetCore.Http;
using Moq;
using System.Text;

namespace LibraryManagement.Test.Services;
public class UsersServiceTests : IClassFixture<UsersServiceFixture>
{
    private readonly UsersServiceFixture _fixture;
    private readonly LibraryDbContext _dbContext;
    private readonly UsersService _usersService;

    public UsersServiceTests(UsersServiceFixture fixture)
    {
        _fixture = fixture;
        _dbContext = _fixture.DbContext;
        _usersService = _fixture.UsersService;
    }

    [Fact]
    public async Task GetUserById_ExistingUser_ReturnsUser()
    {
        //Arrange
        var testUser = new User
        {
            Id = Guid.NewGuid(),
            FirstName = "Test",
            LastName = "User",
            Phone = "0452344534",
            Email = "test@example.com",
            Password = "hashed_password",
            Address = "Kabul",
            AvatarUrl = "https://postimg.cc/BXhdtDmC",
            Salt = new byte[] { (byte)'s', (byte)'a', (byte)'l' },
            Role = UserRole.User,
            CreatedAt = DateTime.UtcNow
        };

        _dbContext.Users.Add(testUser);
        await _dbContext.SaveChangesAsync();

        // Act
        var result = await _usersService.GetByIdAsync(testUser.Id);
        // Assert
        Assert.NotNull(result);
        Assert.Equal(testUser.Id, result.Id);
        Assert.Equal(testUser.Email, result.Email);
        Assert.Equal(testUser.FirstName, result.FirstName);
    }
    [Fact]
    public async Task GetUserByEmail_ExistingUser_ReturnsUser()
    {
        // Arrange
        var testUser = new User
        {
            Id = Guid.NewGuid(),
            FirstName = "Test",
            LastName = "User",
            Phone = "0452344534",
            Email = "test@example.com",
            Password = "hashed_password",
            Address = "Kabul",
            AvatarUrl = "https://postimg.cc/BXhdtDmC",
            Salt = new byte[] { (byte)'s', (byte)'a', (byte)'l' },
            Role = UserRole.User,
            CreatedAt = DateTime.UtcNow
        };

        _dbContext.Add(testUser);
        await _dbContext.SaveChangesAsync();

        // Act
        var result = await _usersService.GetByEmailAsync("test@example.com");

        // Assert
        Assert.NotNull(result);
        Assert.Equal("test@example.com", result.Email);
    }

    [Fact]
    public async Task GetUserById_NonExistingUser_ReturnsNull()
    {
        // Arrange
        var nonExistingId = Guid.NewGuid();
        // Act
        var result = await _usersService.GetByIdAsync(nonExistingId);
        // Assert
        Assert.Null(result);
    }

    [Fact]
    public async Task CreateUser_WhenValidUserIsProvided_ReturnUserReadDto()
    {
        // Arrange
        var fakeFileContent = Encoding.UTF8.GetBytes("fake image content");
        var fakeStream = new MemoryStream(fakeFileContent);
        IFormFile fakeFile = new FormFile(fakeStream, 0, fakeFileContent.Length, "File", "avatar.jpg");


        UserCreateDto userCreateDto = new UserCreateDto
        {
            FirstName = "Test",
            LastName = "User",
            Email = "test.user@example.com",
            Phone = "0452344534",
            Address = "123 Main Street, City, Country",
            Password = "Test@1234",
            File = fakeFile
        };
        var userBuilder = new UserBuilder();

        var userMapped = userBuilder
            .WithId(Guid.NewGuid())
            .WithFirstNameAndLastName(userCreateDto.FirstName, userCreateDto.LastName)
            .WithEmail(userCreateDto.Email)
            .WithPhoneNumber(userCreateDto.Phone)
            .WithAddress(userCreateDto.Address)
            .WithAvatarUrl("https://postimg.cc/BXhdtDmC")
            .WithRole(UserRole.User)
            .WithMemberShipStartAndEndDate(DateTime.UtcNow, DateTime.UtcNow.AddYears(1))
            .Build();

        _fixture.UserMapperMock
            .Setup(m => m.ToEntity(It.IsAny<UserCreateDto>()))
            .Returns(userMapped);

        _fixture.ImageServiceMock
        .Setup(s => s.AddImageAsync(userCreateDto.File))
        .ReturnsAsync(new ImageUploadResult
        {
            SecureUrl = new Uri("https://postimg.cc/BXhdtDmC"),
            PublicId = "fake-public-id",
            Error = null
        });

        _fixture.PasswordServiceMock
                .Setup(s => s.HashPassword(
                    It.IsAny<string>(),
                    out It.Ref<string>.IsAny,
                    out It.Ref<byte[]>.IsAny))
                .Callback((string pw, out string hashed, out byte[] salt) =>
                {
                    hashed = "hashed-password";
                    salt = new byte[16];
                });
        // Act
        var result = await _usersService.CreateUserAsync(userCreateDto);
        await _dbContext.SaveChangesAsync();

        // Assert
        Assert.NotNull(result);
        Assert.True(result.Id != Guid.Empty);
        Assert.Equal(userCreateDto.Email, result.Email);
        Assert.Equal("https://postimg.cc/BXhdtDmC", result.AvatarUrl);
        Assert.Equal(userCreateDto.FirstName, result.FirstName);
        Assert.Equal(userCreateDto.LastName, result.LastName);
        Assert.Equal(userCreateDto.Phone, result.Phone);
        Assert.NotNull(result.AvatarUrl);
    }

    [Fact]
    public async Task CreateAmin_WhenUserRoleIsAdmin_ReturnAdminUser()
    {
        // Arrange
        var fakeFileContent = Encoding.UTF8.GetBytes("fake image content");
        var fakeStream = new MemoryStream(fakeFileContent);
        IFormFile fakeFile = new FormFile(fakeStream, 0, fakeFileContent.Length, "File", "avatar.jpg");


        UserCreateDto userCreateDto = new UserCreateDto
        {
            FirstName = "Test",
            LastName = "User",
            Email = "test.user@example.com",
            Phone = "0452344534",
            Address = "123 Main Street, City, Country",
            Password = "Test@1234",
            File = fakeFile
        };
        var userBuilder = new UserBuilder();

        var userMapped = userBuilder
            .WithId(Guid.NewGuid())
            .WithFirstNameAndLastName(userCreateDto.FirstName, userCreateDto.LastName)
            .WithEmail(userCreateDto.Email)
            .WithPhoneNumber(userCreateDto.Phone)
            .WithAddress(userCreateDto.Address)
            .WithAvatarUrl("https://postimg.cc/BXhdtDmC")
            .WithRole(UserRole.Admin)
            .WithMemberShipStartAndEndDate(DateTime.UtcNow, DateTime.UtcNow.AddYears(1))
            .Build();

        _fixture.UserMapperMock
            .Setup(m => m.ToEntity(It.IsAny<UserCreateDto>()))
            .Returns(userMapped);

        _fixture.ImageServiceMock
        .Setup(s => s.AddImageAsync(userCreateDto.File))
        .ReturnsAsync(new ImageUploadResult
        {
            SecureUrl = new Uri("https://postimg.cc/BXhdtDmC"),
            PublicId = "fake-public-id",
            Error = null
        });

        _fixture.PasswordServiceMock
                .Setup(s => s.HashPassword(
                    It.IsAny<string>(),
                    out It.Ref<string>.IsAny,
                    out It.Ref<byte[]>.IsAny))
                .Callback((string pw, out string hashed, out byte[] salt) =>
                {
                    hashed = "hashed-password";
                    salt = new byte[16];
                });
        // Act
        var result = await _usersService.CreateAdminAsync(userCreateDto);
        await _dbContext.SaveChangesAsync();

        // Assert

        Assert.Equal("Admin", result.Role.ToString());

    }
    [Fact]
    public async Task CreateUser_WhenUserCreateDtoIsNull_ThrowsException()
    {
        // Arrange
        var userCreateDtoNull = (UserCreateDto)null;

        // Act & Assert
        await Assert.ThrowsAsync<ArgumentNullException>(async () =>
        {
            await _usersService.CreateUserAsync(userCreateDtoNull);
        });
    }
    [Fact]
    public async Task CreateUser_WhenImageFileIsNull_ThrowsException()
    {
        // Arrange
        IFormFile fakeFileNull = (IFormFile)null;

        UserCreateDto userCreateDtoNullImage = new UserCreateDto
        {
            FirstName = "Test",
            LastName = "User",
            Email = "test.user@example.com",
            Phone = "0452344534",
            Address = "123 Main Street, City, Country",
            Password = "Test@1234",
            File = fakeFileNull
        };
        // Act & Assert
        await Assert.ThrowsAsync<BusinessRuleViolationException>(async () =>
        {
            await _usersService.CreateUserAsync(userCreateDtoNullImage);
        });
    }
    [Fact]
    public async Task ChangePassword_WhenNewPasswordIsProvided_ChangePassword()
    {
        // Arrange
        var userBuilder = new UserBuilder();

        var existingUser = userBuilder
                .WithId(Guid.NewGuid())
                .WithFirstNameAndLastName("FirstName", "LastName")
                .WithEmail("example@gmail.com")
                .WithPhoneNumber("0453454355")
                .WithAddress("Helsinki")
                .WithPassword("NewPassword@123")
                .WithAvatarUrl("https://postimg.cc/BXhdtDmC")
                .WithRole(UserRole.User)
                .WithMemberShipStartAndEndDate(DateTime.UtcNow, DateTime.UtcNow.AddYears(1))
                .Build();

        _fixture.PasswordServiceMock.Setup(s => s.HashPassword(
                It.IsAny<string>(),
                out It.Ref<string>.IsAny,
                out It.Ref<byte[]>.IsAny))
            .Callback((string pw, out string hashed, out byte[] salt) =>
            {
                hashed = "NewPassword@123";
                salt = new byte[16];
            });

        _dbContext.Users.Add(existingUser);
        // Act
        await _usersService.ChangePassword(existingUser.Id, "NewPassword@123");
        // Assert
        Assert.Equal("NewPassword@123", existingUser.Password);
    }

    [Fact]
    public async Task ChangePassword_WhenUserNotFound_ThrowException()
    {
        // Arrange

        // Act & Assert
        await Assert.ThrowsAsync<BusinessRuleViolationException>(async () =>
         {
             await _usersService.ChangePassword(Guid.NewGuid(), "NewPassword@123");
         });
    }
    [Fact]
    public async Task DeleteUser_WhenExistingUserIdIsProvided_DeletesUser()
    {
        // Arrange
        var userBuilder = new UserBuilder();

        var existingUser = userBuilder
                .WithId(Guid.NewGuid())
                .WithFirstNameAndLastName("FirstName", "LastName")
                .WithEmail("example@gmail.com")
                .WithPhoneNumber("0453454355")
                .WithAddress("Helsinki")
                .WithPassword("NewPassword@123")
                .WithAvatarUrl("https://postimg.cc/BXhdtDmC")
                .WithRole(UserRole.User)
                .WithMemberShipStartAndEndDate(DateTime.UtcNow, DateTime.UtcNow.AddYears(1))
                .Build();
        _dbContext.Users.Add(existingUser);
        // Act
        await _usersService.DeleteByIdAsync(existingUser.Id);
        // Assert
        Assert.True(!_dbContext.Users.Any(u => u.Id == existingUser.Id));
    }
    [Fact]
    public async Task DeleteUserImage_WhenExistingUserIdIsProvided_DeletesUserImage()
    {
        // Arrange
        var userBuilder = new UserBuilder();

        var existingUser = userBuilder
            .WithId(Guid.NewGuid())
            .WithFirstNameAndLastName("FirstName", "LastName")
            .WithEmail("example@gmail.com")
            .WithPhoneNumber("0453454355")
            .WithAddress("Helsinki")
            .WithPassword("NewPassword@123")
            .WithAvatarUrl("https://postimg.cc/BXhdtDmC")
            .WithPublicId("fake-public-id")
            .WithRole(UserRole.User)
            .Build();

        _dbContext.Add(existingUser);
        await _dbContext.SaveChangesAsync();

        _fixture.ImageServiceMock
            .Setup(s => s.DeleteImageAsync(It.IsAny<string>()))
            .ReturnsAsync(new DeletionResult
            {
                Result = "ok"
            });

        // Act
        await _usersService.DeleteByIdAsync(existingUser.Id);

        // Assert
        Assert.False(_dbContext.Users.Any(u => u.Id == existingUser.Id));

        _fixture.ImageServiceMock.Verify(
            s => s.DeleteImageAsync("fake-public-id"),
            Times.Once
        );
    }

    [Fact]
    public async Task DeleteUser_WhenNullGuidIsProvided_ThrowException()
    {
        // Arrange

        // Act & Assert
        await Assert.ThrowsAsync<ArgumentException>(async () =>
          {
              await _usersService.DeleteByIdAsync(Guid.Empty);
          });
    }
    [Fact]
    public async Task DeleteUser_WhenUserNotFound_ThrowException()
    {
        // Arrange
        var userBuilder = new UserBuilder();

        var existingUser = userBuilder
            .WithId(Guid.NewGuid())
            .WithFirstNameAndLastName("FirstName", "LastName")
            .WithEmail("example@gmail.com")
            .WithPhoneNumber("0453454355")
            .WithAddress("Helsinki")
            .WithPassword("NewPassword@123")
            .WithAvatarUrl("https://postimg.cc/BXhdtDmC")
            .WithPublicId("fake-public-id")
            .WithRole(UserRole.User)
            .Build();

        _dbContext.Users.Add(existingUser);
        await _dbContext.SaveChangesAsync();
        // Act & Assert
        await Assert.ThrowsAsync<KeyNotFoundException>(async () =>
          {
              await _usersService.DeleteByIdAsync(Guid.NewGuid());
          });
    }

    [Fact]
    public async Task EntityExistAsync_CheckIfUserExists_ReturnsTrue()
    {
        // Arrange
        var userBuilder = new UserBuilder();
        var existingUser = userBuilder
                    .WithId(Guid.NewGuid())
                    .WithFirstNameAndLastName("FirstName", "LastName")
                    .WithEmail("example@gmail.com")
                    .WithPhoneNumber("0453454355")
                    .WithAddress("Helsinki")
                    .WithPassword("NewPassword@123")
                    .WithAvatarUrl("https://postimg.cc/BXhdtDmC")
                    .WithPublicId("fake-public-id")
                    .WithRole(UserRole.User)
                    .Build();

        _dbContext.Users.Add(existingUser);
        await _dbContext.SaveChangesAsync();

        // Act
        var result = await _usersService.EntityExistAsync(existingUser.Id);
        // Assert
        Assert.True(result);

    }

    [Fact]
    public async Task EntityExistAsync_CheckIfUserDoesNotExist_ReturnsFalse()
    {
        // Arrange

        // Act
        var result = await _usersService.EntityExistAsync(Guid.NewGuid());
        // Assert
        Assert.False(result);

    }

    [Fact]
    public async Task GetUserWithActive_IfActiveLoanExists_ReturnUserWithActiveLoan()
    {
        // Arrange
        var testUser = new User
        {
            Id = Guid.NewGuid(),
            FirstName = "Test",
            LastName = "User",
            Phone = "0452344534",
            Email = "test@example.com",
            Password = "hashed_password",
            Address = "Kabul",
            AvatarUrl = "https://postimg.cc/BXhdtDmC",
            Salt = new byte[] { (byte)'s', (byte)'a', (byte)'l' },
            Role = UserRole.User,
            CreatedAt = DateTime.UtcNow
        };
        _dbContext.Users.Add(testUser);
        await _dbContext.SaveChangesAsync();

        var author = new AuthorBuilder()
     .WithName("F. Scott", "Fitzgerald")
     .WithEmail("fscott@example.com")
     .WithBio("American novelist")
     .Build();

        _dbContext.Authors.Add(author);

        var publisher = new PublisherBuilder()
            .WithName("Scribner")
            .WithAddress("New York")
            .WithWebsite("https://scribner.com")
            .WithEmail("contact@scribner.com")
            .Build();

        _dbContext.Publishers.Add(publisher);

        await _dbContext.SaveChangesAsync();


        var bookBuilder = new BookBuilder();

        var book = bookBuilder
       .WithTitle("The Great Gatsby")
       .WithDescription("A classic novel about the American Dream")
       .WithPublishedDate(new DateTime(1925, 4, 10))
       .WithGenre(Genre.Fiction)
       .WithPages(218)
       .WithCoverImage("https://example.com/great-gatsby.jpg")
       .WithPublisher(publisher.Id)
       .WithAuthor(author.Id)
       .Build();

        _dbContext.Books.Add(book);
        await _dbContext.SaveChangesAsync();

        var testLoan = new Loan
        {
            Id = Guid.NewGuid(),
            UserId = testUser.Id,
            BookId = book.Id,
            LoanDate = DateTime.UtcNow,
            DueDate = DateTime.UtcNow.AddDays(14),
            ReturnDate = null,
            LoanStatus = LoanStatus.Active
        };

        _dbContext.Loans.Add(testLoan);
        await _dbContext.SaveChangesAsync();

        // Act
        var result = await _usersService.GetUsersWithActiveLoansAsync();


        // Assert
        Assert.NotNull(result);
        Assert.Single(result);
        Assert.Equal(testLoan, result.First().Loans.First());
    }
    [Fact]
    public async Task GetUserOverDueLoans_IfOverDueLoansExist_ReturnUserWithOverDueLoan()
    {
        // Arrange
        var testUser = new User
        {
            Id = Guid.NewGuid(),
            FirstName = "Test",
            LastName = "User",
            Phone = "0452344534",
            Email = "test@example.com",
            Password = "hashed_password",
            Address = "Kabul",
            AvatarUrl = "https://postimg.cc/BXhdtDmC",
            Salt = new byte[] { (byte)'s', (byte)'a', (byte)'l' },
            Role = UserRole.User,
            CreatedAt = DateTime.UtcNow
        };
        _dbContext.Users.Add(testUser);
        await _dbContext.SaveChangesAsync();

        var author = new AuthorBuilder()
             .WithName("F. Scott", "Fitzgerald")
             .WithEmail("fscott@example.com")
             .WithBio("American novelist")
             .Build();

        _dbContext.Authors.Add(author);

        var publisher = new PublisherBuilder()
            .WithName("Scribner")
            .WithAddress("New York")
            .WithWebsite("https://scribner.com")
            .WithEmail("contact@scribner.com")
            .Build();

        _dbContext.Publishers.Add(publisher);

        await _dbContext.SaveChangesAsync();


        var bookBuilder = new BookBuilder();

        var book = bookBuilder
           .WithTitle("The Great Gatsby")
           .WithDescription("A classic novel about the American Dream")
           .WithPublishedDate(new DateTime(1925, 4, 10))
           .WithGenre(Genre.Fiction)
           .WithPages(218)
           .WithCoverImage("https://example.com/great-gatsby.jpg")
           .WithPublisher(publisher.Id)
           .WithAuthor(author.Id)
           .Build();

        _dbContext.Books.Add(book);
        await _dbContext.SaveChangesAsync();

        var testLoan = new Loan
        {
            Id = Guid.NewGuid(),
            UserId = testUser.Id,
            BookId = book.Id,
            LoanDate = DateTime.UtcNow,
            DueDate = DateTime.UtcNow.AddDays(14),
            ReturnDate = null,
            LoanStatus = LoanStatus.Overdue
        };

        _dbContext.Loans.Add(testLoan);
        await _dbContext.SaveChangesAsync();

        // Act
        var result = await _usersService.GetUsersWithOverdueLoansAsync();


        // Assert
        Assert.NotNull(result);
        Assert.Single(result);
        Assert.Equal(testLoan, result.First().Loans.First());
    }
    [Fact]
    public async Task ListAllUsersAsync_WithSearchAndPagination_ReturnsFilteredAndPagedUsers()
    {

        // Arrange
        var userList = new List<User>()
        {


                new User
                {
                    Id = Guid.NewGuid(),
                    FirstName = "Test1",
                    LastName = "User1",
                    Phone = "0452344531",
                    Email = "test@example1.com",
                    Password = "hashed_password1",
                    Address = "Kabul",
                    AvatarUrl = "https://postimg.cc/BXhdtDmC",
                    Salt = new byte[] { (byte)'s', (byte)'a', (byte)'l' },
                    Role = UserRole.User,
                    CreatedAt = DateTime.UtcNow
                },
                    new User
                {
                    Id = Guid.NewGuid(),
                    FirstName = "Test2",
                    LastName = "User2",
                    Phone = "0452344532",
                    Email = "test@example2.com",
                    Password = "hashed_password2",
                    Address = "Kabul",
                    AvatarUrl = "https://postimg.cc/BXhdtDmC",
                    Salt = new byte[] { (byte)'s', (byte)'a', (byte)'l' },
                    Role = UserRole.User,
                    CreatedAt = DateTime.UtcNow
                },
                    new User
                {
                    Id = Guid.NewGuid(),
                    FirstName = "Test3",
                    LastName = "User3",
                    Phone = "0452344533",
                    Email = "test@example3.com",
                    Password = "hashed_password3",
                    Address = "Kabul3",
                    AvatarUrl = "https://postimg.cc/BXhdtDmC",
                    Salt = new byte[] { (byte)'s', (byte)'a', (byte)'l' },
                    Role = UserRole.User,
                    CreatedAt = DateTime.UtcNow
                }
            };

        _dbContext.Users.AddRange(userList);
        await _dbContext.SaveChangesAsync();

        var queryOptions = new QueryOptions
        {
            PageNumber = 1,
            PageSize = 2,
            SearchTerm = "Kabul",
            OrderBy = "FirstName",
            IsDescending = false
        };

        // Act
        var result = await _usersService.ListAllUsersAsync(queryOptions);


        // Assert
        Assert.NotNull(result);
        Assert.Equal(2, result.Count);
        Assert.All(result, u => Assert.Contains("Kabul", u.Address));
        Assert.Equal("Test1", result.First().FirstName);
    }

    [Fact]
    public async Task PromoteUserToAdminAsync_WhenUserIdIsProvided_UpdatesUserRoleToAdmin()
    {

        // Arrange
        var user = new User
        {
            Id = Guid.NewGuid(),
            FirstName = "Test1",
            LastName = "User1",
            Phone = "0452344531",
            Email = "test@example1.com",
            Password = "hashed_password1",
            Address = "Kabul1",
            AvatarUrl = "https://postimg.cc/BXhdtDmC",
            Salt = new byte[] { (byte)'s', (byte)'a', (byte)'l' },
            Role = UserRole.User,
            CreatedAt = DateTime.UtcNow
        };

        _dbContext.Users.Add(user);
        await _dbContext.SaveChangesAsync();

        // Act
        var result = await _usersService.PromoteToAdminAsync(user.Id);


        // Assert
        Assert.NotNull(result);
        Assert.Equal(UserRole.Admin, result.Role);
    }
    [Fact]
    public async Task UpdateUserAsync_WithValidData_UpdatesUserSuccessfully()
    {
        // Arrange
        var user = new User
        {
            Id = Guid.NewGuid(),
            FirstName = "Test1",
            LastName = "User1",
            Phone = "0452344531",
            Email = "test@example1.com",
            Password = "hashed_password1",
            Address = "Kabul1",
            AvatarUrl = "https://postimg.cc/BXhdtDmC",
            PublicId = "old-public-id",
            Salt = new byte[] { (byte)'s', (byte)'a', (byte)'l' },
            Role = UserRole.User,
            CreatedAt = DateTime.UtcNow
        };

        _dbContext.Users.Add(user);
        await _dbContext.SaveChangesAsync();

        var newAvatarContent = Encoding.UTF8.GetBytes("fake image content");
        var newFile = new FormFile(new MemoryStream(newAvatarContent), 0, newAvatarContent.Length, "File", "avatar.jpg");

        var userUpdateDto = new UserUpdateDto
        {
            FirstName = "UpdatedFirstName",
            LastName = "UpdatedLastName",
            Address = "Updated Address",
            File = newFile
        };

        _fixture.ImageServiceMock
            .Setup(s => s.DeleteImageAsync(user.PublicId))
            .ReturnsAsync(new DeletionResult { Result = "ok" });

        _fixture.ImageServiceMock
            .Setup(s => s.AddImageAsync(userUpdateDto.File))
            .ReturnsAsync(new ImageUploadResult
            {
                SecureUrl = new Uri("https://postimg.cc/new-avatar"),
                PublicId = "new-public-id",
                Error = null
            });

        _fixture.UserMapperMock
            .Setup(m => m.UpdateFromDto(It.IsAny<User>(), userUpdateDto))
            .Callback<User, UserUpdateDto>((u, dto) =>
            {
                u.FirstName = dto.FirstName;
                u.LastName = dto.LastName;
                u.Address = dto.Address;
            });

        // Act
        var result = await _usersService.UpdateUserAsync(user.Id, userUpdateDto);

        // Assert
        Assert.NotNull(result);
        Assert.Equal("UpdatedFirstName", result.FirstName);
        Assert.Equal("UpdatedLastName", result.LastName);
        Assert.Equal("Updated Address", result.Address);
        Assert.Equal("https://postimg.cc/new-avatar", result.AvatarUrl);
        Assert.Equal("new-public-id", result.PublicId);

        _fixture.ImageServiceMock.Verify(s => s.DeleteImageAsync("old-public-id"), Times.Once);
        _fixture.ImageServiceMock.Verify(s => s.AddImageAsync(userUpdateDto.File), Times.Once);
        _fixture.UserMapperMock.Verify(m => m.UpdateFromDto(It.IsAny<User>(), userUpdateDto), Times.Once);
    }

}