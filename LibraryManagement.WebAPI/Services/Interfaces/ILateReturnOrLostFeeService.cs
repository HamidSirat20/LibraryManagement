using LibraryManagement.WebAPI.Models;
using LibraryManagement.WebAPI.Models.Dtos;

namespace LibraryManagement.WebAPI.Services.Interfaces;
public interface ILateReturnOrLostFeeService
{
    Task<LateReturnOrLostFee> CreateLateFineAsync(LateReturnFineInternalDto dto);
    Task<LateReturnOrLostFee> CreateLostFineAsync(LostFineCreateDto dto);
    Task<LateReturnOrLostFee> GetFineByIdAsync(Guid fineId);
    Task<IEnumerable<LateReturnOrLostFee>> GetFinesForUserAsync(Guid userId);
    Task<IEnumerable<LateReturnOrLostFee>> GetAllAsync();

    Task MarkFinePaidAsync(Guid fineId, DateTime paidDate);
}

