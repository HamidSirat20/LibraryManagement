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
public class ReservationsServiceFixture : IDisposable
{
    public SqliteConnection Connection { get; } 
    public LibraryDbContext DbContext { get; }
    public ReservationService ReservationsService { get; }
    public Mock<IReservationsMapper> ReservationsMapperMock { get; }
    public Mock<IEmailService> EmailServiceMock { get; }
    public Mock<ILogger<ReservationService>> LoggerMock { get; }
    public Mock<IEmailsTemplateService> EmailsTemplateServiceMock { get; }
    public Mock<IReservationsQueueService> ReservationsQueueServiceMock { get; }
    public Mock<IConfiguration> ConfigurationMock { get; }

    public ReservationsServiceFixture()
    {
        Connection = new SqliteConnection("DataSource=:memory:");
        Connection.Open();

        var options = new DbContextOptionsBuilder<LibraryDbContext>()
            .UseSqlite(Connection)
            .EnableSensitiveDataLogging()
            .Options;
        ConfigurationMock = new Mock<IConfiguration>();
        DbContext = new LibraryDbContext(options, ConfigurationMock.Object);
        DbContext.Database.EnsureDeleted();
        DbContext.Database.EnsureCreated();

        LoggerMock = new Mock<ILogger<ReservationService>>();
        ReservationsMapperMock = new Mock<IReservationsMapper>();
        EmailServiceMock = new Mock<IEmailService>();
        EmailsTemplateServiceMock = new Mock<IEmailsTemplateService>();
        ReservationsQueueServiceMock = new Mock<IReservationsQueueService>();

        ReservationsService = new ReservationService(
            DbContext,
            LoggerMock.Object,
            ReservationsMapperMock.Object,
            EmailServiceMock.Object,
            EmailsTemplateServiceMock.Object,
            ReservationsQueueServiceMock.Object
        );
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
        LoggerMock.Reset();
        ReservationsMapperMock.Reset();
        EmailServiceMock.Reset();
        EmailsTemplateServiceMock.Reset();
        ReservationsQueueServiceMock.Reset();
    }
}