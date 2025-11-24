using LibraryManagement.WebAPI.Data;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Moq;

public class SqliteDbFixture : IDisposable
{
    public SqliteConnection Connection { get; }
    public DbContextOptions<LibraryDbContext> Options { get; }
    public DbContext dbContext;
    public IConfiguration Configuration { get; }

    public SqliteDbFixture()
    {
        Connection = new SqliteConnection("Data Source=:memory:");
        Connection.Open();

        var configMock = new Mock<IConfiguration>();
        Configuration = configMock.Object;

        Options = new DbContextOptionsBuilder<LibraryDbContext>()
            .UseSqlite(Connection)
            .EnableSensitiveDataLogging()
            .Options;

        dbContext = new LibraryDbContext(Options, Configuration);
        dbContext.Database.EnsureCreated();
        dbContext.Database.Migrate();
    }

    public void Dispose()
    {
        Connection.Close();
        Connection.Dispose();
        dbContext.Dispose();
    }
}
