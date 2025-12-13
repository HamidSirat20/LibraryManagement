using LibraryManagement.WebAPI.CustomExceptionHandler;
using LibraryManagement.WebAPI.Data;
using LibraryManagement.WebAPI.Events;
using LibraryManagement.WebAPI.Helpers;
using LibraryManagement.WebAPI.Models;
using LibraryManagement.WebAPI.Models.Common;
using LibraryManagement.WebAPI.Models.Dtos;
using LibraryManagement.WebAPI.Services.Interfaces;
using LibraryManagement.WebAPI.Services.ORM.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace LibraryManagement.WebAPI.Services.Implementations;
public class LoansService : ILoansService
{
    private readonly LibraryDbContext _context;
    private readonly ILoansMapper _mapper;
    private readonly ILogger<LoansService> _logger;
    private readonly IReservationsQueueService _reservationQueueService;
    private readonly ILateReturnOrLostFeeService _lateReturnOrLostFeeService;
    private readonly IEventAggregator _eventAggregator;
    public LoansService(LibraryDbContext context, ILoansMapper mapper, ILogger<LoansService> logger, IReservationsQueueService reservationQueueService, ILateReturnOrLostFeeService lateReturnOrLostFeeService, IEventAggregator eventAggregator)
    {
        _context = context ?? throw new ArgumentNullException(nameof(context));
        _mapper = mapper ?? throw new ArgumentNullException(nameof(mapper));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _reservationQueueService = reservationQueueService ?? throw new ArgumentNullException(nameof(reservationQueueService));
        _lateReturnOrLostFeeService = lateReturnOrLostFeeService ?? throw new ArgumentNullException(nameof(lateReturnOrLostFeeService));
        _eventAggregator = eventAggregator ?? throw new ArgumentNullException(nameof(eventAggregator));
    }

    public async Task<LoanReadDto> MakeLoanAsync(LoanCreateDto loanCreateDto, Guid userId)
    {
        using var _ = _logger.BeginScope("Making a new loan for BookId: {BookId}",
            loanCreateDto.BookId);
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
                .AsNoTracking()
                .Include(b => b.Loans)
                .Include(b => b.Reservations)
                .FirstOrDefaultAsync(b => b.Id == loanCreateDto.BookId)
            ?? throw new KeyNotFoundException($"Book with ID {nameof(loanCreateDto.BookId)} not found.");

            if (!book.IsAvailable)
            {
                _logger.LogWarning("The requested book is currently unavailable. Suggesting reservation option.");
                throw new BusinessRuleViolationException(
                    $"Book with ID {loanCreateDto.BookId} is currently unavailable for loan.You may place a reservation to be notified when it becomes available."
                );
            }


            var loan = new Loan
            {
                UserId = userId,
                BookId = loanCreateDto.BookId,
                LoanDate = DateTime.UtcNow,
                DueDate = DateTime.UtcNow.AddDays(30),
                ReturnDate = null,
                LoanStatus = LoanStatus.Active
            };

            _context.Loans.Add(loan);
            await _context.SaveChangesAsync();

            var loanWithDetails = await _context.Loans
                                                .Include(l => l.User)
                                                .Include(l => l.Book)
                                                .Include(l => l.LateReturnOrLostFees)
                                                .FirstOrDefaultAsync(l => l.Id == loan.Id);

            var loanReadDto = _mapper.ToLoanReadDto(loanWithDetails);

            _logger.LogDebug("Created new loan with ID: {LoanId}", loan.Id);
            return loanReadDto;
        }
        catch (BusinessRuleViolationException)
        {
            throw;
        }
        catch (KeyNotFoundException)
        {
            throw;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "An error occurred while making a new loan.");
            throw;
        }
    }

    public async Task<IEnumerable<Loan>> GetYourOwnLoansAsync(Guid userId, bool includeReturnedLoans = false)
    {

        if (userId == Guid.Empty)
            throw new ArgumentException("User ID cannot be empty", nameof(userId));

        try
        {
            var query = _context.Loans
                .Where(l => l.UserId == userId)
                .Include(l => l.Book)
                .Include(l => l.User)
                .AsNoTracking()
                .OrderByDescending(l => l.LoanDate)
                .AsQueryable();

            if (!includeReturnedLoans)
            {
                query = query.Where(l => l.LoanStatus == LoanStatus.Active) ?? throw new KeyNotFoundException($"No active loans found for {userId} user.");
            }

            var loans = await query.ToListAsync() ?? throw new KeyNotFoundException($"No loans found for {userId} user.");

            _logger.LogInformation("Retrieved {LoanCount} loans for user {UserId}",
                loans.Count, userId);

            return loans;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving loans for user {UserId}", userId);
            throw;
        }
    }

    public async Task<IEnumerable<Loan>> GetLoansByUserIdAsync(Guid userId, bool includeReturnedLoans = false)
    {
        if (userId == Guid.Empty)
            throw new ArgumentException("User ID cannot be empty", nameof(userId));
        try
        {
            var query = _context.Loans
              .Where(l => l.UserId == userId)
              .Include(l => l.Book)
              .Include(l => l.User)
              .AsNoTracking()
              .OrderByDescending(l => l.LoanDate)
              .AsQueryable();

            if (!includeReturnedLoans)
            {
                query = query.Where(l => l.LoanStatus == LoanStatus.Active) ?? throw new KeyNotFoundException($"No loans found for {userId} user.");
            }

            var loans = await query.ToListAsync() ?? throw new KeyNotFoundException("No loans found for this user.");
            return loans;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving loans for user {UserId}", userId);
            throw;
        }
    }

    public async Task<PaginatedResponse<Loan>> GetAllLoansAsync(QueryOptions queryOptions)
    {
        try
        {
            var query = _context.Loans
                            .Include(l => l.Book)
                            .Include(l => l.User)
                            .AsNoTracking()
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
                                         l.User.LastName.ToLower().Contains(searchTerm) ||
                                         l.User.Email.ToLower().Contains(searchTerm) ||
                                         l.LoanStatus.ToString().ToLower().Contains(searchTerm));
            }

            var paginatedLoans = await PaginatedResponse<Loan>.CreateAsync(query, queryOptions.PageNumber, queryOptions.PageSize);
            return paginatedLoans;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "An error occurred while retrieving all loans.");
            throw;
        }

    }

    public async Task<Loan> GetLoanByIdAsync(Guid loanId)
    {
        if (loanId == Guid.Empty)
            throw new ArgumentException("Loan ID cannot be empty", nameof(loanId));
        try
        {
            var loan = await _context.Loans.Include(u => u.User)
                                            .Include(r => r.Book)
                                            .AsNoTracking()
                                            .FirstOrDefaultAsync(l => l.Id == loanId);
            if (loan is null)
            {
                _logger.LogWarning("Loan with {loan.Id} does not exist!", loanId);
                throw new BusinessRuleViolationException($"Book with {loanId} id not found", "Not_Found");
            }
            _logger.LogInformation("Loan with Id {loan.Id} returned!", loan.Id);
            return loan;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving loan with ID {LoanId}", loanId);
            throw;
        }
    }

    public async Task<IEnumerable<Loan>> GetOverdueLoansAsync()
    {
        try
        {
            var overdueLoans = await _context.Loans
            .Where(l => l.DueDate < DateTime.UtcNow && l.LoanStatus == LoanStatus.Active)
            .Include(l => l.User)
            .Include(l => l.Book)
            .AsNoTracking()
            .ToListAsync();

            if (!overdueLoans.Any())
            {
                _logger.LogInformation("No overdue loans found at {Time}.", DateTime.UtcNow);
                return Enumerable.Empty<Loan>();
            }

            _logger.LogInformation("Retrieved {Count} overdue loans.", overdueLoans.Count);
            return overdueLoans;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "An error occurred while retrieving overdue loans.");
            throw;
        }
    }
    public async Task<Loan> ReturnBookAsync(Guid loanId)
    {
        if (loanId == Guid.Empty)
            throw new ArgumentException("Loan ID cannot be empty", nameof(loanId));
        try
        {
            var returnLoan = await _context.Loans
                                    .Include(l => l.Book)
                                    .Include(l => l.User)
                                    .FirstOrDefaultAsync(l => l.Id == loanId);
            if (returnLoan is null)
            {
                _logger.LogWarning($"Loan with {loanId} id not found.");
                throw new ArgumentNullException($"Loan with {loanId} id not found.");
            }

            // Update loan status to Returned and set return date
            var (user, book, dueDate, returnDate, status) = returnLoan;

            returnLoan.LoanStatus = LoanStatus.Returned;
            returnLoan.ReturnDate = DateTime.UtcNow;

            // Calculate late fee if applicable and create a LateReturnOrLostFee
            var endDate = returnLoan.ReturnDate!.Value;
            var lateDays = (endDate - dueDate).Days;
            var fee = returnLoan.CalculateLateFee();

            if (fee > 0)
            {
                var lateFee = new LateReturnFineInternalDto
                {
                    LoanId = returnLoan.Id,
                    UserId = user.Id,
                    Amount = fee,
                    Description = $"Late return fee for loan {book.Title}, {lateDays} day(s) late."
                };

                await _lateReturnOrLostFeeService.CreateLateFineAsync(lateFee);
            }

            _logger.LogInformation("Loan with id {loanId} returned.", loanId);

            // Find the next reservation in queue for this book
            var nextReservation = await _context.Reservations
                .Include(r => r.User)
                .Include(r => r.Book)
                .Where(r => r.BookId == returnLoan.BookId &&
                           r.ReservationStatus == ReservationStatus.Pending)
                .OrderBy(r => r.QueuePosition)
                .FirstOrDefaultAsync();

            //send email notification to user
            if (nextReservation != null)
            {
                await _eventAggregator.PublishAsync(new ReservationReadyEventArgs
                {
                    FirstName = nextReservation.User.FirstName,
                    LastName = nextReservation.User.LastName,
                    BookTitle = nextReservation.Book.Title,
                    PickUpDeadLine = DateTime.UtcNow.AddDays(3),
                    UserEmail = nextReservation.User.Email,
                });

                await _reservationQueueService.ProcessNextReservationAfterReturnAsync(nextReservation.BookId);
            }
            _context.Loans.Update(returnLoan);
            await _context.SaveChangesAsync();

            return returnLoan;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error returning loan with ID {LoanId}", loanId);
            throw;
        }
    }

    public async Task<Loan> UpdateLoanAsync(Guid loanId)
    {
        try
        {

            var loanToUpdate = await GetLoanByIdAsync(loanId);

            // Check membership validity
            var user = loanToUpdate.User ?? throw new KeyNotFoundException($"User with ID {loanToUpdate.UserId} not found.");

            if (!user.IsActive)
            {
                throw new BusinessRuleViolationException(
                    "Your membership has expired. Please renew before borrowing books.",
                    "MEMBERSHIP_EXPIRED");
            }
            // Cannot extend overdue loans
            if (loanToUpdate.LoanStatus == LoanStatus.Overdue)
            {
                throw new BusinessRuleViolationException(
                    "You cannot extend this loan because it is already overdue.",
                    "LOAN_OVERDUE");
            }
            // Check if reservation for the book exists
            bool hasActiveReservation = await _context.Reservations
                .AnyAsync(r =>
                    r.BookId == loanToUpdate.BookId &&
                    (r.ReservationStatus == ReservationStatus.Pending ||
                     r.ReservationStatus == ReservationStatus.Notified));
            if (hasActiveReservation)
            {
                _logger.LogWarning("Loan with {loan.Id} cannot be extended!", loanToUpdate.Id);
                throw new InvalidOperationException("You can not extend this book since it is reserved by someone.");
            }
            // Extend the due date by 30 days
            loanToUpdate.DueDate = loanToUpdate.DueDate.AddDays(30);
            await _context.SaveChangesAsync();
            return loanToUpdate;
        }
        catch (BusinessRuleViolationException)
        {
            throw;
        }
        catch (KeyNotFoundException)
        {
            throw;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating loan with ID {LoanId}", loanId);
            throw;
        }
    }

}
