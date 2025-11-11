using LibraryManagement.WebAPI.Data;
using LibraryManagement.WebAPI.Helpers;
using LibraryManagement.WebAPI.Models;
using LibraryManagement.WebAPI.Models.Common;
using LibraryManagement.WebAPI.Models.Dtos;
using LibraryManagement.WebAPI.Services.Interfaces;
using LibraryManagement.WebAPI.Services.ORM.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace LibraryManagement.WebAPI.Services.Implementations;
public class LoanService : ILoanService
{
    private readonly LibraryDbContext _context;
    private readonly ILoanMapper _mapper;
    private readonly ILogger<LoanService> _logger;
    public LoanService(LibraryDbContext context, ILoanMapper mapper, ILogger<LoanService> logger, IHttpContextAccessor httpContextAccessor)
    {
        _context = context ?? throw new ArgumentNullException(nameof(context));
        _mapper = mapper ?? throw new ArgumentNullException(nameof(mapper));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public async Task<Loan> MakeLoanAsync(LoanCreateDto loanCreateDto)
    {
        using var _ = _logger.BeginScope("Making a new loan for BookId: {BookId} by UserId: {UserId}",
            loanCreateDto.BookId, loanCreateDto.UserId);

        var book = await _context.Books
            .AsNoTracking()
            .Include(b => b.Loans)
            .Include(b => b.Reservations)
            .FirstOrDefaultAsync(b => b.Id == loanCreateDto.BookId);

        if (book == null)
            throw new KeyNotFoundException($"Book with ID {loanCreateDto.BookId} not found.");

        if (!book.IsAvailable)
        {
            _logger.LogWarning("This book not available for loan now.");
            throw new InvalidOperationException($"Book with ID {loanCreateDto.BookId} is not available.");
        }

        var loan = _mapper.ToLoan(loanCreateDto) ??
                   throw new InvalidOperationException("Mapping failed.");

        loan.LoanStatus = LoanStatus.Active;

        _context.Loans.Add(loan);
        await _context.SaveChangesAsync();

        _logger.LogInformation("Created new loan with ID: {LoanId}", loan.Id);
        return loan;
    }

    public async Task<IEnumerable<Loan>> GetYourOwnLoansAsync(Guid userId, bool includeReturnedLoans = false)
    {
        try
        {
            if (!includeReturnedLoans)
            {
                var userLoans = await _context.Loans.Where(u => u.UserId == userId).Where(l => l.LoanStatus == LoanStatus.Active).ToListAsync();
                if (userLoans == null)
                    throw new KeyNotFoundException("No loans found for this user.");
                return userLoans;
            }
            else
            {
                var userLoans = await _context.Loans.Where(u => u.UserId == userId).ToListAsync();
                if (userLoans == null)
                    throw new KeyNotFoundException("No loans found for this user.");
                return userLoans;
            }
        }
        catch {
            throw new ArgumentNullException();
        }
    }

    public async Task<IEnumerable<Loan>> GetLoansByUserIdAsync(Guid userId, bool includeReturnedLoans = false)
    {
        try
        {
            if (!includeReturnedLoans)
            {
                var userLoans = await _context.Loans.Where(u => u.UserId == userId).Where(l => l.LoanStatus == LoanStatus.Active).ToListAsync();
                if (userLoans == null)
                    throw new KeyNotFoundException("No loans found for this user.");
                return userLoans;
            }
            else
            {
                var userLoans = await _context.Loans.Where(u => u.UserId == userId).ToListAsync();
                if (userLoans == null)
                    throw new KeyNotFoundException("No loans found for this user.");
                return userLoans;
            }
        }
        catch
        {
            throw new ArgumentNullException();
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
        try
        {
            var loan = await _context.Loans.Include(u=>u.User)
                                            .Include(r=>r.Book)
                                            .FirstOrDefaultAsync(l => l.Id == loanId);
            if (loan == null)
            {
                _logger.LogWarning("Loan with {loan.Id} does not exist!", loan.Id);
                throw new ArgumentNullException(nameof(loan));
            }
            _logger.LogInformation("Loan with Id {loan.Id} returned!",loan.Id);
            return loan;
        }
        catch(Exception ex) 
        {
            throw new Exception(ex.Message); 
        }
    }

    public async Task<IEnumerable<Loan>> GetOverdueLoansAsync()
    {
        var overdueLoans = await _context.Loans
            .Where(l => l.DueDate < DateTime.UtcNow && l.LoanStatus == LoanStatus.Active)
            .Include(l => l.User)
            .Include(l => l.Book)
            .ToListAsync();

        if (!overdueLoans.Any())
        {
            _logger.LogInformation("No overdue loans found at {Time}.", DateTime.UtcNow);
            return Enumerable.Empty<Loan>(); 
        }

        _logger.LogInformation("Retrieved {Count} overdue loans.", overdueLoans.Count);
        return overdueLoans;
    }

    public async Task<Loan> ReturnBookAsync(Guid loanId)
    {
        try
        {
            var returnLoan = await GetLoanByIdAsync(loanId);
            if (returnLoan != null)
            {

                returnLoan.LoanStatus = LoanStatus.Returned;
                returnLoan.ReturnDate = DateTime.UtcNow;
                returnLoan.CalculateLateFee();
                await _context.SaveChangesAsync();
                _logger.LogInformation("Loan with id {loanId} returned.", loanId);
                return returnLoan;
            }
            else
            {
                throw new ArgumentNullException(nameof(loanId));
            }
        }
        catch (Exception ex) 
        {
           throw new Exception(ex.Message, ex);
        }
    }

    public async Task<Loan> UpdateLoanAsync(Guid loanId)
    {
        try
        {
            var loanToUpdate = await GetLoanByIdAsync(loanId);

            var reservation = await _context.Reservations
                .Where(r => r.BookId == loanToUpdate.BookId && r.ReservationStatus == ReservationStatus.Pending)
                .OrderBy(r => r.ReservedAt)
                .ToListAsync();

            bool hasActiveReservation = reservation.Any(r =>
                                        r.ReservationStatus == ReservationStatus.Pending ||
                                        r.ReservationStatus == ReservationStatus.Notified);
            if (hasActiveReservation)
            {
                _logger.LogWarning("Loan with {loan.Id} does not exist!",loanToUpdate.Id);
                throw new Exception("You can not extend this book since it is reserved by someone.");           
            }
            loanToUpdate.DueDate = loanToUpdate.DueDate.AddDays(30);
            await _context.SaveChangesAsync();
            return loanToUpdate;
        }
        catch (Exception ex)
        {
            throw new Exception(ex.Message);
        }

    }

}
