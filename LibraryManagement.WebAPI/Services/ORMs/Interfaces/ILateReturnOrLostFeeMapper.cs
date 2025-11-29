using LibraryManagement.WebAPI.Models;
using LibraryManagement.WebAPI.Models.Dtos;

namespace LibraryManagement.WebAPI.Services.ORM.Interfaces;

public interface ILateReturnOrLostFeeMapper
{
    LateReturnOrLostFeeReadDto ToReadDto(LateReturnOrLostFee fee);
    LateReturnOrLostFeeCreateDto ToCreateDto(LateReturnOrLostFee fee);
    LateReturnOrLostFeeUpdateDto ToUpdateDto(LateReturnOrLostFee fee);

    LateReturnOrLostFee ToEntity(LateReturnOrLostFeeCreateDto dto);
    LateReturnOrLostFee UpdateFromDto(LateReturnOrLostFee fee, LateReturnOrLostFeeUpdateDto dto);
}
