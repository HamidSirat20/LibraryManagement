using LibraryManagement.WebAPI.Data;
using LibraryManagement.WebAPI.Events;
using LibraryManagement.WebAPI.Services.Implementations;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Moq;

namespace LibraryManagement.Test.Fixtures;
public class LateReturnOrLostFeeServiceFixture
{
    private SqliteConnection Connection;
    public LibraryDbContext DbContext { get; }
    public Mock<IEventAggregator> EventAggregatorMock;
    public Mock<ILogger<LateReturnOrLostFeeService>> LoggerMock { get; }
    public Mock<IConfiguration> ConfigurationMock { get; }
    public LateReturnOrLostFeeService LateReturnOrLostFee { get; }

    public LateReturnOrLostFeeServiceFixture()
    {
        Connection = new SqliteConnection("Data Source =:memory");
        Connection.Open();

        EventAggregatorMock = new Mock<IEventAggregator>();
        LoggerMock = new Mock<ILogger<LateReturnOrLostFeeService>>();
        ConfigurationMock = new Mock<IConfiguration>();

        var option = new DbContextOptionsBuilder<LibraryDbContext>()
            .UseSqlite(Connection)
            .EnableSensitiveDataLogging()
            .Options;
        DbContext.Database.EnsureDeleted();
        DbContext.Database.EnsureCreated();

        DbContext = new LibraryDbContext(option, ConfigurationMock.Object);
        LateReturnOrLostFee = new LateReturnOrLostFeeService(DbContext, LoggerMock.Object, EventAggregatorMock.Object);

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
        LoggerMock.Reset();
    }
}
