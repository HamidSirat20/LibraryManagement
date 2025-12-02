using LibraryManagement.WebAPI.CustomExceptionHandler;
using LibraryManagement.WebAPI.Data;
using LibraryManagement.WebAPI.Models;
using LibraryManagement.WebAPI.Models.Dtos;
using LibraryManagement.WebAPI.Services.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace LibraryManagement.WebAPI.Services.Implementations;
public class LateReturnOrLostFeeService : ILateReturnOrLostFeeService
{
    private readonly LibraryDbContext _dbContext;
    private readonly ILogger<LateReturnOrLostFeeService> _logger;
    public LateReturnOrLostFeeService(LibraryDbContext libraryDbContext, ILogger<LateReturnOrLostFeeService> logger)
    {
        _dbContext = libraryDbContext ?? throw new ArgumentNullException(nameof(libraryDbContext));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }
    public async Task<LateReturnOrLostFee> CreateLateFineAsync(LateReturnFineInternalDto dto)
    {
        try
        {
            if (dto == null)
            {
                throw new ArgumentNullException(nameof(dto), "LateReturnOrLostFeeCreateDto cannot be null.");
            }
            var fine = new LateReturnOrLostFee
            {
                UserId = dto.UserId,
                LoanId = dto.LoanId,
                Amount = dto.Amount,
                FineType = FineType.LateReturn,
                Status = FineStatus.Pending,
                IssuedDate = DateTime.UtcNow,
                Description = dto.Description ?? "Late return fee overdue book."
            };

            _dbContext.LateReturnOrLostFees.Add(fine);
            await _dbContext.SaveChangesAsync();

            return fine;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "An error occurred while creating a fine.");
            throw;
        }
    }

    public async Task<LateReturnOrLostFee> CreateLostFineAsync(LostFineCreateDto dto)
    {

        try
        {
            var loan = await _dbContext.Loans
            .Include(l => l.Book)
            .FirstOrDefaultAsync(l => l.Id == dto.LoanId);


            if (loan == null)
                throw new BusinessRuleViolationException("Loan not found for lost item fee.");

            // Check if already marked lost
            if (loan.Book.BookStatus == BookStatus.Lost)
                throw new BusinessRuleViolationException("This book is already marked as lost. A fine already exists.");

            var fine = new LateReturnOrLostFee
            {
                UserId = dto.UserId,
                LoanId = dto.LoanId,
                Amount = dto.Amount,
                FineType = FineType.LostItem,
                Status = FineStatus.Pending,
                IssuedDate = DateTime.UtcNow,
                Description = dto.Description ?? "Lost book fee"
            };

            _dbContext.LateReturnOrLostFees.Add(fine);

            // Mark book as Lost
            loan.Book.BookStatus = BookStatus.Lost;

            await _dbContext.SaveChangesAsync();


            return fine;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "An error occurred while creating a lost fine.");
            throw;
        }

    }

    public async Task<IEnumerable<LateReturnOrLostFee>> GetAllAsync()
    {
        try
        {
            return await _dbContext.LateReturnOrLostFees.ToListAsync();
        }

        catch (BusinessRuleViolationException)
        {
            throw;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "An error occurred while retrieving all fines.");
            throw;
        }
    }

    public async Task<LateReturnOrLostFee> GetFineByIdAsync(Guid id)
    {
        try
        {
            var fine = await _dbContext.LateReturnOrLostFees.FirstOrDefaultAsync(f => f.Id == id);
            if (fine == null)
            {
                _logger.LogWarning("Fine with id {FineId} not found.", id);
                throw new BusinessRuleViolationException($"Fine with id {id} not found.", "Not_Found");
            }
            return fine;
        }
        catch (BusinessRuleViolationException)
        {
            throw;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "An error occurred while retrieving a fine by id.");
            throw;
        }
    }

    public async Task<IEnumerable<LateReturnOrLostFee>> GetFinesForUserAsync(Guid userId)
    {
        try
        {
            var fines = _dbContext.LateReturnOrLostFees.Where(f => f.UserId == userId).AsQueryable();
            return await fines.ToListAsync();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "An error occurred while retrieving fines for user id {UserId}.", userId);
            throw;
        }
    }

    public async Task MarkFinePaidAsync(Guid fineId, DateTime paidDate)
    {
        try
        {
            var fine = _dbContext.LateReturnOrLostFees.Find(fineId);
            if (fine == null)
            {
                _logger.LogWarning("Fine with id {FineId} not found for marking as paid.", fineId);
                throw new BusinessRuleViolationException($"Fine with id {fineId} not found.", "Not_Found");
            }
            fine.Status = FineStatus.Paid;
            fine.PaidDate = paidDate;
            await _dbContext.SaveChangesAsync();
        }
        catch (BusinessRuleViolationException)
        {
            throw;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "An error occurred while marking fine id {FineId} as paid.", fineId);
            throw;
        }
    }
}
