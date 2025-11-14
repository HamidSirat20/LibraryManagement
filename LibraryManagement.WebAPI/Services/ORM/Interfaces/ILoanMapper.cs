using LibraryManagement.WebAPI.Models;
using LibraryManagement.WebAPI.Models.Dtos;

namespace LibraryManagement.WebAPI.Services.ORM.Interfaces;
public interface ILoanMapper
{
    LoanReadDto ToLoanReadDto(Loan loan);
    LoanUpdateDto ToLoanUpdateDto(Loan loan);
    Loan ToLoan(LoanCreateDto loanCreateDto, Guid userId); 
    Loan UpdateFromDto(Loan loan, LoanUpdateDto loanUpdateDto);

}