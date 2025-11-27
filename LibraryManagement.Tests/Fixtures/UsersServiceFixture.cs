using LibraryManagement.WebAPI.Data;
using LibraryManagement.WebAPI.Services.Implementations;
using LibraryManagement.WebAPI.Services.Interfaces;
using LibraryManagement.WebAPI.Services.ORM.Interfaces;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Moq;

namespace LibraryManagement.Test.Fixtures;

public class UsersServiceFixture : IDisposable
{
    public SqliteConnection Connection { get; }
    public UsersService UsersService { get; }
    public LibraryDbContext DbContext { get; }
    public Mock<IUsersMapper> UserMapperMock { get; }
    public Mock<IImageService> ImageServiceMock { get; }
    public Mock<IPasswordService> PasswordServiceMock { get; }
    public Mock<ILogger<UsersService>> LoggerMock { get; }

    public UsersServiceFixture()
    {
        Connection = new SqliteConnection("Data Source=:memory:");
        Connection.Open();

        var dbOptions = new DbContextOptionsBuilder<LibraryDbContext>()
            .UseSqlite(Connection)
            .Options;

        var configMock = new Mock<IConfiguration>();

        DbContext = new LibraryDbContext(dbOptions, configMock.Object);
        DbContext.Database.EnsureDeleted();
        DbContext.Database.EnsureCreated();

        UserMapperMock = new Mock<IUsersMapper>();

        PasswordServiceMock = new Mock<IPasswordService>();
        ImageServiceMock = new Mock<IImageService>();
        LoggerMock = new Mock<ILogger<UsersService>>();

        UsersService = new UsersService(
            DbContext,
            UserMapperMock.Object,
            PasswordServiceMock.Object,
            ImageServiceMock.Object,
            LoggerMock.Object
        );
    }

    public void Dispose()
    {
        Connection?.Close();
        Connection?.Dispose();
        DbContext?.Dispose();
    }
    public void Reset()
    {
        // Clear all tables
        DbContext.Loans.RemoveRange(DbContext.Loans);
        DbContext.Reservations.RemoveRange(DbContext.Reservations);
        DbContext.Users.RemoveRange(DbContext.Users);
        DbContext.Books.RemoveRange(DbContext.Books);
        DbContext.Authors.RemoveRange(DbContext.Authors);
        DbContext.Publishers.RemoveRange(DbContext.Publishers);

        DbContext.SaveChanges();

        // Reset mocks
        UserMapperMock.Reset();
        PasswordServiceMock.Reset();
        ImageServiceMock.Reset();
        LoggerMock.Reset();
    }
}