using LibraryManagement.WebAPI.Models;
using LibraryManagement.WebAPI.Models.Dtos;

namespace LibraryManagement.WebAPI.Services.ORM.Interfaces;
public interface ILoanMapper
{
    LoanReadDto ToLoanReadDto(Loan loan);
    LoanCreateDto ToLoanCreateDto(Loan loan);
    LoanUpdateDto ToLoanUpdateDto(Loan loan);
    Loan ToLoan(LoanCreateDto loanCreateDto);
    Loan UpdateFromDto(Loan loan, LoanUpdateDto loanUpdateDto);
}