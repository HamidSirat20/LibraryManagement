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
public class LoansServiceFixture : IDisposable
{
    public SqliteConnection Connection { get; }
    public LibraryDbContext DbContext { get; }
    public Mock<ILoansMapper> LoansMapperMock { get; }
    public Mock<IEmailsTemplateService> EmailsTemplateServiceMock { get; }
    public Mock<IReservationsQueueService> ReservationQueueServiceMock { get; }
    public Mock<ILogger<LoansService>> LoggerMock { get; }
    public Mock<IConfiguration> ConfigurationMock { get; }
    public Mock<ILateReturnOrLostFeeService> LateReturnOrLostFeeServiceMock { get; }
    public LoansService LoansService { get; }
    public LoansServiceFixture()
    {
        Connection = new SqliteConnection("DataSource=:memory:");
        Connection.Open();
        var options = new DbContextOptionsBuilder<LibraryDbContext>()
            .UseSqlite(Connection)
            .Options;

        ConfigurationMock = new Mock<IConfiguration>();
        DbContext = new LibraryDbContext(options, ConfigurationMock.Object);

        DbContext.Database.EnsureDeleted();
        DbContext.Database.EnsureCreated();
        LoansMapperMock = new Mock<ILoansMapper>();
        EmailsTemplateServiceMock = new Mock<IEmailsTemplateService>();
        ReservationQueueServiceMock = new Mock<IReservationsQueueService>();
        LoggerMock = new Mock<ILogger<LoansService>>();
        LateReturnOrLostFeeServiceMock = new Mock<ILateReturnOrLostFeeService>();
        LoansService = new LoansService(
            DbContext, LoansMapperMock.Object,
            LoggerMock.Object, EmailsTemplateServiceMock.Object,
            ReservationQueueServiceMock.Object, LateReturnOrLostFeeServiceMock.Object);

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
        LoansMapperMock.Reset();
        EmailsTemplateServiceMock.Reset();
        ReservationQueueServiceMock.Reset();
        LoggerMock.Reset();
    }

}
