using LibraryManagement.WebAPI.Data;
using LibraryManagement.WebAPI.Models;
using LibraryManagement.WebAPI.Services.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace LibraryManagement.WebAPI.Services.Implementations;

public class ReservationsQueueService : IReservationsQueueService
{
    private readonly LibraryDbContext _context;
    private readonly IEmailService _emailService;
    private readonly ILogger<ReservationsQueueService> _logger;
    public ReservationsQueueService(
        LibraryDbContext context,
        IEmailService emailService,
        ILogger<ReservationsQueueService> logger)
    {
        _context = context ?? throw new ArgumentNullException(nameof(context));
        _emailService = emailService ?? throw new ArgumentNullException(nameof(emailService));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public async Task ProcessNextReservationAfterReturnAsync(Guid bookId, string emailSubject, string emailBody)
    {
        try
        {
            // Find the next reservation in queue for this book
            var nextReservation = await _context.Reservations
                .Include(r => r.User)
                .Include(r => r.Book)
                .Where(r => r.BookId == bookId &&
                           r.ReservationStatus == ReservationStatus.Pending)
                .OrderBy(r => r.QueuePosition)
                .FirstOrDefaultAsync();

            if (nextReservation != null)
            {
                // Update reservation status and notify user
                nextReservation.ReservationStatus = ReservationStatus.Notified;
                nextReservation.QueuePosition = 0;
                nextReservation.UpdatedAt = DateTime.UtcNow;
                var userEmail = nextReservation.User.Email;
                // Send notification email
                await _emailService.SendEmailAsync(userEmail, emailSubject, emailBody);

                _logger.LogInformation("Notified user {UserId} about ready reservation {ReservationId}",
                    nextReservation.UserId, nextReservation.Id);

                // Reorder the remaining queue
                await ReorderReservationQueueAsync(bookId);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to process next reservation for book {BookId}", bookId);

        }
    }

    private async Task ReorderReservationQueueAsync(Guid bookId)
    {
        var pendingReservations = await _context.Reservations
           .Where(r => r.BookId == bookId && r.ReservationStatus == ReservationStatus.Pending)
           .OrderBy(r => r.ReservedAt)
           .ToListAsync();

        for (int i = 0; i < pendingReservations.Count; i++)
        {
            pendingReservations[i].QueuePosition = i + 1;
        }

        await _context.SaveChangesAsync();

        _logger.LogInformation("Reordered queue for book {BookId}, now has {Count} pending reservations",
            bookId, pendingReservations.Count);
    }

    public async Task<int> GetQueuePositionAsync(Guid bookId, Guid reservationId)
    {
        var reservation = await _context.Reservations
            .FirstOrDefaultAsync(r => r.Id == reservationId && r.BookId == bookId);

        return reservation?.QueuePosition ?? -1;
    }
}

