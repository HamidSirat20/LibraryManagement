using LibraryManagement.WebAPI.Data;
using LibraryManagement.WebAPI.Services.Implementations;
using LibraryManagement.WebAPI.Services.ORM.Interfaces;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Moq;

namespace LibraryManagement.Test.Fixtures;
public class AuthorsServiceFixture : IDisposable
{
    public LibraryDbContext DbContext { get; }
    public SqliteConnection Connection { get; }
    public Mock<ILogger<AuthorsService>> LoggerMock { get; }
    public Mock<IAuthorsMapper> AuthorMapperMock { get; }
    public Mock<IConfiguration> ConfigurationMock { get; }
    public AuthorsService AuthorsService { get; }
    public AuthorsServiceFixture()
    {
        Connection = new SqliteConnection("Data Source = :memory:");
        Connection.Open();

        var options = new DbContextOptionsBuilder<LibraryDbContext>()
            .UseSqlite(Connection)
            .EnableSensitiveDataLogging()
            .Options;

        ConfigurationMock = new Mock<IConfiguration>();
        DbContext = new LibraryDbContext(options, ConfigurationMock.Object);
        DbContext.Database.EnsureDeleted();
        DbContext.Database.EnsureCreated();

        LoggerMock = new Mock<ILogger<AuthorsService>>();

        AuthorMapperMock = new Mock<IAuthorsMapper>();

        AuthorsService = new AuthorsService(DbContext, LoggerMock.Object, AuthorMapperMock.Object);

    }
    public void Dispose()
    {
        Connection.Close();
        Connection.Dispose();
        DbContext.Dispose();
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
        AuthorMapperMock.Reset();
        ConfigurationMock.Reset();
        LoggerMock.Reset();
    }
}
