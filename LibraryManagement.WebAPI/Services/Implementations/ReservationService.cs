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
    private readonly IReservationMapper _reservationMapper;
    public ReservationService(LibraryDbContext context, ILogger<ReservationService> logger, IReservationMapper reservationMapper)
    {
        _context = context ?? throw new ArgumentNullException(nameof(context));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _reservationMapper = reservationMapper ?? throw new ArgumentNullException(nameof(reservationMapper));
    }


    public async Task<ReservationReadDto?> CreateReservationAsync(Guid bookId, Guid userId)
    {
        try
        {
            var book = await _context.Books
                            .Include(l=>l.Loans)
                            .Include(r=>r.Reservations)
                            .FirstOrDefaultAsync(b => b.Id == bookId);
            if (book == null)
            {
                _logger.LogError("The book was not found for reservation!");
                throw new BusinessRuleViolationException(
                                                       $"Book with ID {bookId} not found.",
                                                       "BOOK_NOT_FOUND");
            }
            // Check if the book is available for resrvation
            if (book.IsAvailable)
            {
                _logger.LogInformation("Book {BookId} is available; no need to reserve.", bookId);
                throw new BusinessRuleViolationException(
                                                         "Book is currently available for loan — no need to reserve.",
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
                throw new BusinessRuleViolationException(
                                                       "You already have a reservation for this book.",
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
                ReservedAt = DateTime.Now,
                ReservationStatus = ReservationStatus.Pending,
                QueuePosition = nextQueuePosition
            };

            _context.Reservations.Add(reservation);
            await _context.SaveChangesAsync();

            var dto = _reservationMapper.ToReservationReadDto(reservation);


            _logger.LogInformation("Reservation created for Book {BookId} by User {UserId}.", bookId, userId);
            return dto;
        }
        catch(DbUpdateException ex)
        {
            _logger.LogError(ex.Message,"There was some mistake while making a reservation");
            throw new BusinessRuleViolationException(
                                                    "A system error occurred while creating the reservation. Please try again.",
                                                    "DATABASE_ERROR");
        }
        catch (BusinessRuleViolationException)
        {
            throw;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unexpected error in CreateReservationAsync");
            throw new BusinessRuleViolationException(
                "An unexpected error occurred while processing your reservation.",
                "UNEXPECTED_ERROR");
        }
    }
    public Task<Reservation> DeleteReservationAsync(Guid id)
    {
        throw new NotImplementedException();
    }

    public async Task<ReservationReadDto?> PickReservationByIdAsync(Guid reservationId, Guid currentUserId)
    {
        try
        {
            var reservation = await _context.Reservations.Include(u=>u.User).FirstOrDefaultAsync(r=>r.Id == reservationId);
            if (reservation == null)
            {
                _logger.LogWarning("Reservation with ID {ReservationId} not found", reservationId);
                return null;
            }


            if (reservation.UserId != currentUserId)
            {
                _logger.LogWarning("User {UserId} does not own reservation {ReservationId}", currentUserId, reservationId);
                throw new BusinessRuleViolationException("User does not own this reservation","No_Reservation");
            }

 
            if (reservation.ReservationStatus != ReservationStatus.Pending)
            {
                _logger.LogWarning("Reservation {ReservationId} is not active. Current status: {Status}",
                    reservationId, reservation.ReservationStatus);
                throw new InvalidOperationException($"Cannot pickup reservation with status: {reservation.ReservationStatus}");
            }

            var book = await _context.Books.FirstOrDefaultAsync(b =>b.Id == reservation.BookId);
            if (book == null)
            {
                throw new KeyNotFoundException($"Book with ID {reservation.BookId} not found");
            }

            if (!book.IsAvailableForPickUp)
            {
                throw new BusinessRuleViolationException("Book is not available for loan","Not-For-Pickup");
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

            await CompleteReservationAsync(reservationId);
            reservation.UpdatedAt = DateTime.UtcNow;
            var updatedReservation = _context.Reservations.Update(reservation);
            _context.Loans.Add(loan);

            await _context.SaveChangesAsync();

            _logger.LogInformation("Successfully picked up reservation {ReservationId} and created loan {LoanId}",
                reservationId, loan.Id);

            var reservationDto = _reservationMapper.ToReservationReadDto(updatedReservation.Entity);
            return reservationDto;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error picking up reservation {ReservationId}", reservationId);
            throw;
        }
    }

    public Task<PaginatedResponse<ReservationReadDto>> ListAllReservationAsync(QueryOptions queryOptions)
    {
        throw new NotImplementedException();
    }

    public Task<PaginatedResponse<ReservationReadDto>> ListReservationForAUserAsync()
    {
        throw new NotImplementedException();
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
    public async Task CompleteReservationAsync(Guid reservationId)
    {
        var reservation = await _context.Reservations.FindAsync(reservationId);
        if (reservation == null)
            throw new KeyNotFoundException($"Reservation with ID {reservationId} not found.");

        int? oldPosition = reservation.QueuePosition;

        reservation.ReservationStatus = ReservationStatus.Fulfilled;

        reservation.QueuePosition = 0; 

        await _context.SaveChangesAsync();

        await ReorderReservationQueueAsync(reservation.BookId);
    }


}

