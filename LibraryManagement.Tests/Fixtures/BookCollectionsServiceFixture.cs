using LibraryManagement.WebAPI.Data;
using LibraryManagement.WebAPI.Services.Implementations;
using LibraryManagement.WebAPI.Services.ORM.Interfaces;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Moq;

namespace LibraryManagement.Test.Fixtures;
public class BookCollectionsServiceFixture : IDisposable
{
    public SqliteConnection Connection { get; }
    public LibraryDbContext DbContext { get; }
    public BookCollectionsService BookCollectionsService { get; }
    public Mock<IBooksMapper> BookMapperMock;
    public Mock<IConfiguration> Configuration { get; }

    public BookCollectionsServiceFixture()
    {
        Connection = new SqliteConnection("Data Source=:memory:");
        Connection.Open();
        var options = new DbContextOptionsBuilder<LibraryDbContext>()
            .UseSqlite(Connection)
            .Options;

        Configuration = new Mock<IConfiguration>();

        DbContext = new LibraryDbContext(options, Configuration.Object);
        DbContext.Database.EnsureDeleted();
        DbContext.Database.EnsureCreated();

        BookMapperMock = new Mock<IBooksMapper>();
        BookCollectionsService = new BookCollectionsService(DbContext, BookMapperMock.Object);
    }
    public void Dispose()
    {
        Connection.Close();
        Connection.Dispose();
        DbContext.Dispose();
    }
}
