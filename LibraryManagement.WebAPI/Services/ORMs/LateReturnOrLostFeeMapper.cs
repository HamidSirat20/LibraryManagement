using LibraryManagement.WebAPI.Models;
using LibraryManagement.WebAPI.Models.Dtos;
using LibraryManagement.WebAPI.Services.ORM.Interfaces;

namespace LibraryManagement.WebAPI.Services.ORM;

public class LateReturnOrLostFeeMapper : ILateReturnOrLostFeeMapper
{
    public LateReturnOrLostFeeReadDto ToReadDto(LateReturnOrLostFee fee)
    {
        if (fee == null) return null;

        return new LateReturnOrLostFeeReadDto
        {
            Id = fee.Id,
            UserId = fee.UserId,
            UserName = fee.User != null
                ? $"{fee.User.FirstName} {fee.User.LastName}"
                : null,

            LoanId = fee.LoanId,
            BookTitle = fee.Loan?.Book?.Title,

            FineType = fee.FineType,
            Amount = fee.Amount,
            IssuedDate = fee.IssuedDate,
            PaidDate = fee.PaidDate,
            Status = fee.Status,
            Description = fee.Description
        };
    }

    public LateReturnOrLostFeeCreateDto ToCreateDto(LateReturnOrLostFee fee)
    {
        if (fee == null) return null;

        return new LateReturnOrLostFeeCreateDto
        {
            UserId = fee.UserId,
            LoanId = fee.LoanId,
            Amount = fee.Amount,
            IssuedDate = fee.IssuedDate,
            Status = fee.Status,
            FineType = fee.FineType

        };
    }

    public LateReturnOrLostFeeUpdateDto ToUpdateDto(LateReturnOrLostFee fee)
    {
        if (fee == null) return null;

        return new LateReturnOrLostFeeUpdateDto
        {
            Amount = fee.Amount,
            PaidDate = fee.PaidDate,
            Status = fee.Status,
            FineType = fee.FineType
        };
    }

    public LateReturnOrLostFee ToEntity(LateReturnOrLostFeeCreateDto dto)
    {
        if (dto == null) return null;

        return new LateReturnOrLostFee
        {
            UserId = dto.UserId,
            LoanId = dto.LoanId,
            Amount = dto.Amount,
            IssuedDate = dto.IssuedDate,
            Status = dto.Status,
            FineType = dto.FineType,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };
    }

    public LateReturnOrLostFee UpdateFromDto(LateReturnOrLostFee fee, LateReturnOrLostFeeUpdateDto dto)
    {
        if (dto == null) return fee;

        if (dto.Amount.HasValue)
            fee.Amount = dto.Amount.Value;

        if (dto.PaidDate.HasValue)
            fee.PaidDate = dto.PaidDate.Value;

        if (dto.Status.HasValue)
            fee.Status = dto.Status.Value;

        if (dto.FineType.HasValue)
            fee.FineType = dto.FineType.Value;

        fee.UpdatedAt = DateTime.UtcNow;

        return fee;
    }
}
