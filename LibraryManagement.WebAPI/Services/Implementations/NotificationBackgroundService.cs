using LibraryManagement.WebAPI.Data;
using LibraryManagement.WebAPI.Models;
using LibraryManagement.WebAPI.Services.Interfaces;
using Microsoft.EntityFrameworkCore;

public class NotificationBackgroundService : BackgroundService
{
    private readonly IServiceScopeFactory _scopeFactory;

    public NotificationBackgroundService(IServiceScopeFactory scopeFactory)
    {
        _scopeFactory = scopeFactory;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        var timer = new PeriodicTimer(TimeSpan.FromHours(8));

        while (await timer.WaitForNextTickAsync(stoppingToken))
        {
            await ProcessDueDateNotificationsAsync(stoppingToken);
            await ProcessFineFeeNotificationsAsync(stoppingToken);
        }
    }


    private async Task ProcessDueDateNotificationsAsync(CancellationToken stoppingToken)
    {
        using var scope = _scopeFactory.CreateScope();

        var dbContext = scope.ServiceProvider.GetRequiredService<LibraryDbContext>();
        var emailService = scope.ServiceProvider.GetRequiredService<IEmailService>();
        var templateService = scope.ServiceProvider.GetRequiredService<IEmailsTemplateService>();

        var notificationTime = DateTime.UtcNow.AddDays(2);

        var dueLoans = await dbContext.Loans
            .Include(l => l.Book)
            .Include(l => l.User)
            .Where(l => l.LoanStatus == LoanStatus.Active &&
                        l.DueDate <= notificationTime)
            .ToListAsync(stoppingToken);

        foreach (var loan in dueLoans)
        {
            var body = templateService.GetReturnReminderTemplate(
                loan.User.FirstName,
                loan.User.LastName,
                loan.Book.Title,
                loan.DueDate,
                2
            );

            await emailService.SendEmailAsync(
                loan.User.Email,
                $"Reminder: {loan.Book.Title} is due soon",
                body
            );
            if (loan.DueDate < DateTime.UtcNow)
            {
                loan.LoanStatus = LoanStatus.Overdue;
            }
        }
        await dbContext.SaveChangesAsync(stoppingToken);

    }
    private async Task ProcessFineFeeNotificationsAsync(CancellationToken stoppingToken)
    {
        using var scope = _scopeFactory.CreateScope();

        var dbContext = scope.ServiceProvider.GetRequiredService<LibraryDbContext>();
        var emailService = scope.ServiceProvider.GetRequiredService<IEmailService>();
        var templateService = scope.ServiceProvider.GetRequiredService<IEmailsTemplateService>();

        // Get all pending fines
        var fines = await dbContext.LateReturnOrLostFees
            .Include(f => f.User)
            .Where(f => f.Status == FineStatus.Pending)
            .AsTracking()
            .ToListAsync(stoppingToken);

        foreach (var fine in fines)
        {
            var emailBody = templateService.GetPaymentReminderTemplate(
               fine.User.FirstName, fine.User.LastName, fine.Loan.Book.Title,
               fine.FineType, fine.Amount
            );

            try
            {
                await emailService.SendEmailAsync(
                    fine.User.Email,
                    $"Reminder: {fine.FineType} payment is due soon",
                    emailBody
                );
                fine.Status = FineStatus.Notified;
                await dbContext.SaveChangesAsync(stoppingToken);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Failed to send email to {fine.User.Email}: {ex.Message}");
            }
        }
    }

}
