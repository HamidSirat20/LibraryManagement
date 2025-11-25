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
public class BooksServiceFixture : IDisposable
{
    public SqliteConnection Connection { get; }
    public LibraryDbContext DbContext { get; }
    public BooksService BooksService { get; }
    public Mock<IConfiguration> ConfigurationMock { get; }
    public Mock<IBooksMapper> BookMapperMock;
    public Mock<IImageService> ImageServiceMock;
    public Mock<ILogger<BooksService>> LoggerMock;
    public BooksServiceFixture()
    {
        Connection = new SqliteConnection("Data Source=:memory:");
        Connection.Open();

        var options = new DbContextOptionsBuilder<LibraryDbContext>()
            .UseSqlite(Connection)
            .EnableSensitiveDataLogging()
            .Options;

        ConfigurationMock = new Mock<IConfiguration>();
        DbContext = new LibraryDbContext(options, ConfigurationMock.Object);
        DbContext.Database.EnsureDeleted();
        DbContext.Database.EnsureCreated();

        BookMapperMock = new Mock<IBooksMapper>();
        ImageServiceMock = new Mock<IImageService>();
        LoggerMock = new Mock<ILogger<BooksService>>();

        BooksService = new BooksService(DbContext, 
                                        BookMapperMock.Object,
                                        ImageServiceMock.Object,
                                        LoggerMock.Object); 
    }
    public void Dispose()
    {
        Connection.Close();
        Connection.Dispose();
        DbContext.Dispose();
    }
}
