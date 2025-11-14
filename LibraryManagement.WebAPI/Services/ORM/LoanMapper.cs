using LibraryManagement.WebAPI.Models;
using LibraryManagement.WebAPI.Models.Dtos;
using LibraryManagement.WebAPI.Services.ORM.Interfaces;

namespace LibraryManagement.WebAPI.Services.ORM;

public class LoanMapper : ILoanMapper
{
    public LoanReadDto ToLoanReadDto(Loan loan)
    {
        if (loan == null) return null!;

        return new LoanReadDto
        {
            Id = loan.Id,
            UserId = loan.UserId,
            UserName = loan.User?.FullName ?? "Unknown User",
            BookId = loan.BookId,
            BookTitle = loan.Book?.Title ?? "Unknown Book",
            LoanDate = loan.LoanDate,
            DueDate = loan.DueDate,
            ReturnDate = loan.ReturnDate,
            LoanStatus = loan.LoanStatus,
            LateFee = loan.LateFee,
            LateReturnOrLostFees = loan.LateReturnOrLostFees?
                .Select(fee => new LateReturnOrLostFeeReadDto
                {
                    Id = fee.Id,
                    UserId = fee.UserId,
                    UserName = fee.User?.FullName ?? "Unknown User",
                    LoanId = fee.LoanId,
                    BookTitle = fee.Loan?.Book?.Title ?? "Unknown Book",
                    Amount = fee.Amount,
                    IssuedDate = fee.IssuedDate,
                    PaidDate = fee.PaidDate,
                    Status = fee.Status
                }).ToList() ?? new List<LateReturnOrLostFeeReadDto>()
        };
    }

    public LoanUpdateDto ToLoanUpdateDto(Loan loan)
    {
        if (loan == null) return null!;

        return new LoanUpdateDto
        {
            DueDate = loan.DueDate,
            ReturnDate = loan.ReturnDate,
            LoanStatus = loan.LoanStatus
        };
    }

    public Loan ToLoan(LoanCreateDto dto, Guid userId)
    {
        if (dto == null) return null!;

        return new Loan
        {
            Id = Guid.NewGuid(),
            UserId = userId,
            BookId = dto.BookId,
            LoanDate = DateTime.UtcNow,
            DueDate = DateTime.UtcNow.AddDays(30),
            LoanStatus = LoanStatus.Active,
            LateFee = 0
        };
    }

    public Loan UpdateFromDto(Loan loan, LoanUpdateDto dto)
    {
        if (dto == null) return loan;

        if (dto.DueDate.HasValue)
            loan.DueDate = dto.DueDate.Value;

        if (dto.ReturnDate.HasValue)
            loan.ReturnDate = dto.ReturnDate.Value;

        if (dto.LoanStatus.HasValue)
            loan.LoanStatus = dto.LoanStatus.Value;

        return loan;
    }
}
