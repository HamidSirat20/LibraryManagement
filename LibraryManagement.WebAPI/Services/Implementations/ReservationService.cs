using LibraryManagement.WebAPI.CustomExceptionHandler;
using LibraryManagement.WebAPI.Data;
using LibraryManagement.WebAPI.Helpers;
using LibraryManagement.WebAPI.Models;
using LibraryManagement.WebAPI.Models.Common;
using LibraryManagement.WebAPI.Models.Dtos;
using LibraryManagement.WebAPI.Services.Interfaces;
using LibraryManagement.WebAPI.Services.ORM.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace LibraryManagement.WebAPI.Services.Implementations;

public class ReservationService : IReservationService
{
    private readonly LibraryDbContext _context;
    private readonly ILogger<ReservationService> _logger;
    private readonly IReservationsMapper _reservationMapper;
    private readonly IEmailService _emailService;
    private readonly IEmailsTemplateService _emailTemplateService;
    private readonly IReservationsQueueService _reservationQueueService;
    public ReservationService(LibraryDbContext context, ILogger<ReservationService> logger, IReservationsMapper reservationMapper, IEmailService emailService, IEmailsTemplateService emailTemplateService, IReservationsQueueService reservationQueueService)
    {
        _context = context ?? throw new ArgumentNullException(nameof(context));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _reservationMapper = reservationMapper ?? throw new ArgumentNullException(nameof(reservationMapper));
        _emailService = emailService ?? throw new ArgumentNullException(nameof(emailService));
        _emailTemplateService = emailTemplateService ?? throw new ArgumentNullException(nameof(emailTemplateService));
        _reservationQueueService = reservationQueueService ?? throw new ArgumentNullException(nameof(reservationQueueService));
    }


    public async Task<ReservationReadDto?> CreateReservationAsync(Guid bookId, Guid userId)
    {
        try
        {
            // Check membership validity
            var user = await _context.Users
           .AsNoTracking()
           .FirstOrDefaultAsync(u => u.Id == userId)
           ?? throw new KeyNotFoundException($"User with ID {userId} not found.");

            if (!user.IsActive)
            {
                throw new BusinessRuleViolationException(
                    "Your membership has expired. Please renew before borrowing books.",
                    "MEMBERSHIP_EXPIRED");
            }
            var book = await _context.Books
                            .Include(l => l.Loans)
                            .Include(r => r.Reservations)
                            .FirstOrDefaultAsync(b => b.Id == bookId);
            if (book == null)
            {
                _logger.LogError("The book was not found for reservation!");
                throw new BusinessRuleViolationException($"Book with this {bookId} id not found.",
                                                         "BOOK_NOT_FOUND");
            }
            // Check if the book is available for reservation
            if (book.IsAvailable)
            {
                _logger.LogInformation("Book {BookId} is available for immediate checkout - no reservation needed.", bookId);
                throw new BusinessRuleViolationException("Book is available for immediate checkout - no reservation needed.",
                                                         "BOOK_AVAILABLE");
            }
            // Check if the user already has an active reservation for the book
            var existingReservation = await _context.Reservations
                                         .FirstOrDefaultAsync(r => r.UserId == userId &&
                                                                   r.BookId == bookId &&
                                                                   (r.ReservationStatus == ReservationStatus.Pending ||
                                                                    r.ReservationStatus == ReservationStatus.Notified));
            if (existingReservation != null)
            {
                _logger.LogWarning("User {UserId} already has a reservation for Book {BookId}.", userId, bookId);
                throw new BusinessRuleViolationException("You already have a reservation for this book.",
                                                          "DUPLICATE_RESERVATION");
            }


            var existingReservations = await _context.Reservations
                .Where(r => r.BookId == bookId && r.ReservationStatus == ReservationStatus.Pending)
                .OrderBy(r => r.ReservedAt)
                .ToListAsync();

            int nextQueuePosition = existingReservations.Count + 1;


            var reservation = new Reservation
            {
                BookId = bookId,
                UserId = userId,
                ReservedAt = DateTime.UtcNow,
                ReservationStatus = ReservationStatus.Pending,
                QueuePosition = nextQueuePosition
            };

            _context.Reservations.Add(reservation);
            await _context.SaveChangesAsync();

            var reservationReadDto = _reservationMapper.ToReservationReadDto(reservation);

            var newReservation = await _context.Reservations
                      .Include(r => r.User)
                      .Include(r => r.Book)
                      .FirstOrDefaultAsync(r => r.Id == reservation.Id);

            // build email body
            var reservationCreatedAt = reservation.CreatedAt ?? DateTime.UtcNow;
            var body = _emailTemplateService.GetReservationConfirmationTemplate(newReservation.User.FullName, newReservation.User.LastName, newReservation.Book.Title
                , reservationCreatedAt, newReservation.QueuePosition);
            await _emailService.SendEmailAsync(reservation.User.Email, "Reservation Created", body);

            _logger.LogInformation("Reservation created for Book {BookId} by User {UserId}.", bookId, userId);
            return reservationReadDto;
        }
        catch (DbUpdateException ex)
        {
            _logger.LogError(ex.Message, "There was some mistake while making a reservation.");
            throw new DbUpdateException("A system error occurred while creating the reservation. Please try again.");
        }
        catch (Exception ex) when (ex is not BusinessRuleViolationException)
        {
            _logger.LogError(ex, "Unexpected error while creating reservation.");
            throw new Exception("An unexpected error occurred while processing your reservation.");
        }
    }
    public async Task<Reservation> CancelReservationAsync(Guid reservationId, Guid userId)
    {
        try
        {
            var reservation = await _context.Reservations
                                            .Include(u => u.User)
                                            .FirstOrDefaultAsync(r => r.Id == reservationId);
            if (reservation == null)
            {
                _logger.LogError("The reservation with {id} not found.", reservationId);
                throw new BusinessRuleViolationException($"The reservation with {reservationId} not found.", "NOT_FOUND");
            }
            if (reservation.UserId != userId)
            {
                _logger.LogError("User {UserId} is not authorized to cancel reservation {ReservationId}.", userId, reservationId);
                throw new BusinessRuleViolationException("You are not authorized to cancel this reservation.", "UNAUTHORIZED_CANCEL");
            }

            // reorder the queue after cancellation
            await CompleteReservationAsync(reservationId);

            reservation.ReservationStatus = ReservationStatus.Cancelled;
            reservation.UpdatedAt = DateTime.UtcNow;
            await _context.SaveChangesAsync();
            return reservation;
        }
        catch (BusinessRuleViolationException)
        {
            throw;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"There was a problem with {reservationId} cancellation.");
            throw new Exception($"There was a problem with canceling {reservationId} reservation.");
        }
    }

    public async Task<ReservationReadDto?> PickReservationByIdAsync(Guid reservationId, Guid currentUserId)
    {
        try
        {
            // Check membership validity
            var user = await _context.Users
           .AsNoTracking()
           .FirstOrDefaultAsync(u => u.Id == currentUserId)
           ?? throw new KeyNotFoundException($"User with ID {currentUserId} not found.");

            if (!user.IsActive)
            {
                throw new BusinessRuleViolationException(
                    "Your membership has expired. Please renew before borrowing books.",
                    "MEMBERSHIP_EXPIRED");
            }
            var reservation = await _context.Reservations.Include(u => u.User).FirstOrDefaultAsync(r => r.Id == reservationId);
            if (reservation == null)
            {
                _logger.LogWarning("Reservation with ID {ReservationId} not found", reservationId);
                throw new BusinessRuleViolationException($"The reservation with {reservationId} id not found.", "RESERVATION_NOT_FOUND");
            }


            if (reservation.UserId != currentUserId)
            {
                _logger.LogWarning("User {UserId} does not own reservation {ReservationId}", currentUserId, reservationId);
                throw new BusinessRuleViolationException("User does not own this reservation.", "UNAUTHORIZED_PICKUP");
            }


            if (reservation.ReservationStatus != ReservationStatus.Pending && reservation.ReservationStatus != ReservationStatus.Notified)
            {
                _logger.LogWarning("Reservation {ReservationId} is not active. Current status: {Status}",
                    reservationId, reservation.ReservationStatus);
                throw new BusinessRuleViolationException($"Cannot pickup reservation with status: {reservation.ReservationStatus}",
                                                           "INVALID_RESERVATION_STATUS");
            }

            var book = await _context.Books.FirstOrDefaultAsync(b => b.Id == reservation.BookId);
            if (book == null)
            {
                throw new BusinessRuleViolationException(
                    $"Book with ID {reservation.BookId} not found",
                    "BOOK_NOT_FOUND");
            }

            if (!book.IsAvailableForPickUp)
            {
                throw new BusinessRuleViolationException(
                    "Book is not available for loan",
                    "BOOK_NOT_AVAILABLE");
            }


            var loan = new Loan
            {
                BookId = reservation.BookId,
                UserId = currentUserId,
                LoanDate = DateTime.UtcNow,
                DueDate = DateTime.UtcNow.AddDays(30),
                ReturnDate = null,
                LoanStatus = LoanStatus.Active
            };

            // await CompleteReservationAsync(reservationId);
            reservation.ReservationStatus = ReservationStatus.Fulfilled;
            reservation.UpdatedAt = DateTime.UtcNow;
            var updatedReservation = _context.Reservations.Update(reservation);
            _context.Loans.Add(loan);

            await _context.SaveChangesAsync();

            _logger.LogInformation("Successfully picked up reservation {ReservationId} and created loan {LoanId}",
                reservationId, loan.Id);

            var reservationDto = _reservationMapper.ToReservationReadDto(updatedReservation.Entity);
            return reservationDto;
        }
        catch (BusinessRuleViolationException)
        {
            throw;
        }
        catch (KeyNotFoundException)
        {
            throw;
        }
        catch (Exception ex) when (ex is not BusinessRuleViolationException)
        {
            _logger.LogError(ex, "Unexpected error picking up reservation {ReservationId}", reservationId);
            throw new Exception("An error occurred while processing your pickup request", ex);
        }
    }

    public async Task<PaginatedResponse<Reservation>> ListAllReservationAsync(QueryOptions queryOptions)
    {
        try
        {
            var query = _context
                .Reservations
                .Include(b => b.Book)
                .Include(b => b.User)
                .Where(r => r.ReservationStatus == ReservationStatus.Pending)
                .AsQueryable();

            // Apply sort
            if (!string.IsNullOrWhiteSpace(queryOptions.SearchTerm))
            {
                var sortBy = queryOptions.OrderBy.Trim().ToLower();
                query = query.ApplySorting(sortBy, queryOptions.IsDescending, "LoanDate");

            }
            //apply search
            if (!string.IsNullOrWhiteSpace(queryOptions.SearchTerm))
            {
                var searchTerm = queryOptions.SearchTerm.Trim().ToLower();
                query = query.Where(l => l.Book.Title.ToLower().Contains(searchTerm) ||
                                         l.User.FirstName.ToLower().Contains(searchTerm) ||
                                         l.User.LastName.ToLower().Contains(searchTerm));
            }

            var paginatedLoans = await PaginatedResponse<Reservation>.CreateAsync(query, queryOptions.PageNumber, queryOptions.PageSize);
            return paginatedLoans;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error listing reservations");
            throw;
        }
    }

    public async Task<IEnumerable<Reservation>> ListReservationForAUserAsync(Guid userId)
    {
        try
        {
            var reservations = _context
                .Reservations
                .Include(b => b.Book)
                .Where(r => r.UserId == userId)
                .Where(r => r.ReservationStatus == ReservationStatus.Pending)
                .AsQueryable();
            if (!reservations.Any())
            {
                _logger.LogWarning("No reservations found for user {UserId}", userId);
                throw new BusinessRuleViolationException("No reservations found for the user.", "NO_RESERVATIONS");
            }
            return reservations;

        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error listing reservations for user {UserId}", userId);
            throw new Exception("An error occurred while retrieving your reservations.");
        }
    }
    private async Task ReorderReservationQueueAsync(Guid bookId)
    {
        var reservations = await _context.Reservations
            .Where(r => r.BookId == bookId && r.ReservationStatus == ReservationStatus.Pending)
            .OrderBy(r => r.ReservedAt)
            .ToListAsync();

        for (int i = 0; i < reservations.Count; i++)
        {
            reservations[i].QueuePosition = i + 1;
        }

        await _context.SaveChangesAsync();
    }
    private async Task CompleteReservationAsync(Guid reservationId)
    {
        var reservation = await _context.Reservations.FindAsync(reservationId);
        if (reservation == null)
            throw new KeyNotFoundException($"Reservation with ID {reservationId} not found.");

        int? oldPosition = reservation.QueuePosition;

        reservation.QueuePosition = 0;

        await _context.SaveChangesAsync();

        await ReorderReservationQueueAsync(reservation.BookId);
    }


}

