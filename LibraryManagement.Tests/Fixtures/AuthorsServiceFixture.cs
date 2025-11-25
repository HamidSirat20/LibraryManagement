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

        LoggerMock = new Mock<ILogger<AuthorsService>>();

        AuthorMapperMock = new Mock<IAuthorsMapper>();

    }
    public void Dispose()
    {
        Connection.Close();
        Connection.Dispose();
        DbContext.Dispose();
    }
}
