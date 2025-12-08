using LibraryManagement.WebAPI.Helpers;
using LibraryManagement.WebAPI.Models;
using LibraryManagement.WebAPI.Models.Common;
using LibraryManagement.WebAPI.Models.Dtos;

namespace LibraryManagement.WebAPI.Services.Interfaces;
public interface ILoansService
{
    Task<LoanReadDto> MakeLoanAsync(LoanCreateDto loanCreateDto, Guid userId);
    Task<Loan> GetLoanByIdAsync(Guid loanId);
    Task<PaginatedResponse<Loan>> GetAllLoansAsync(QueryOptions queryOptions);
    Task<Loan> UpdateLoanAsync(Guid loanId);

    Task<Loan> ReturnBookAsync(Guid loanId);
    Task<IEnumerable<Loan>> GetYourOwnLoansAsync(Guid userId, bool includeReturnedLoans = false);
    Task<IEnumerable<Loan>> GetLoansByUserIdAsync(Guid userId, bool includeReturnedLoans = false);
    Task<IEnumerable<Loan>> GetOverdueLoansAsync();
}

