using LibraryManagement.WebAPI.Data;
using LibraryManagement.WebAPI.Services.Implementations;
using LibraryManagement.WebAPI.Services.ORM.Interfaces;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Moq;

namespace LibraryManagement.Test.Fixtures;
public class PublishersServiceFixture : IDisposable
{
    public SqliteConnection Connection { get; }
    public LibraryDbContext DbContext { get; }
    public PublishersService PublishersService { get; }
    public Mock<IPublishersMapper> PublishersMapperMock { get; }
    public Mock<ILogger<PublishersService>> LoggerMock { get; }
    public Mock<IConfiguration> ConfigurationMock { get; }
    public PublishersServiceFixture()
    {
        Connection = new SqliteConnection("Data Source =:memory:");
        Connection.Open();

        var options = new DbContextOptionsBuilder<LibraryDbContext>()
            .UseSqlite(Connection)
            .EnableSensitiveDataLogging()
            .Options;

        ConfigurationMock = new Mock<IConfiguration>();
        DbContext = new LibraryDbContext(options, ConfigurationMock.Object);

        DbContext.Database.EnsureDeleted();
        DbContext.Database.EnsureCreated();

        PublishersMapperMock = new Mock<IPublishersMapper>();
        LoggerMock = new Mock<ILogger<PublishersService>>();

        PublishersService = new PublishersService(DbContext, PublishersMapperMock.Object,LoggerMock.Object);     
    }

    public void Dispose()
    {
        Connection.Close();
        Connection.Dispose();
        DbContext.Dispose();
    }
    public void Reset()
    {
        DbContext.ChangeTracker.Clear();

        DbContext.Database.EnsureDeleted();
        DbContext.Database.EnsureCreated();

        // Reset mocks
        PublishersMapperMock.Reset();
        LoggerMock.Reset();
        ConfigurationMock.Reset();
    }
}
