using LibraryManagement.WebAPI.Data;
using LibraryManagement.WebAPI.Models;
using LibraryManagement.WebAPI.Models.Dtos;
using LibraryManagement.WebAPI.Services.Interfaces;
using LibraryManagement.WebAPI.Services.ORM.Interfaces;

namespace LibraryManagement.WebAPI.Services.Implementations;
public class LateReturnOrLostFeeService : ILateReturnOrLostFeeService
{
    private readonly LibraryDbContext _dbContext;
    private readonly ILogger<LateReturnOrLostFeeService> _logger;
    private readonly ILateReturnOrLostFeeMapper _mapper;

    public LateReturnOrLostFeeService(LibraryDbContext libraryDbContext, ILogger<LateReturnOrLostFeeService> logger, ILateReturnOrLostFeeMapper mapper)
    {
        _dbContext = libraryDbContext ?? throw new ArgumentNullException(nameof(libraryDbContext));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _mapper = mapper ?? throw new ArgumentNullException(nameof(mapper));
    }
    public async Task<LateReturnOrLostFee> CreateFineAsync(LateReturnOrLostFeeCreateDto dto)
    {
        try
        {
            if (dto == null)
            {
                throw new ArgumentNullException(nameof(dto), "LateReturnOrLostFeeCreateDto cannot be null.");
            }
            var fine = _mapper.ToEntity(dto);
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

    public Task<bool> DeleteFineAsync(Guid id)
    {
        throw new NotImplementedException();
    }

    public Task<IEnumerable<LateReturnOrLostFee>> GetAllAsync()
    {
        throw new NotImplementedException();
    }

    public Task<LateReturnOrLostFee> GetFineByIdAsync(Guid id)
    {
        throw new NotImplementedException();
    }

    public Task<IEnumerable<LateReturnOrLostFee>> GetFinesForUserAsync(Guid userId)
    {
        throw new NotImplementedException();
    }

    public Task MarkFinePaidAsync(Guid fineId, DateTime paidDate)
    {
        throw new NotImplementedException();
    }

    public Task<LateReturnOrLostFee> UpdateFineAsync(Guid id, LateReturnOrLostFeeUpdateDto dto)
    {
        throw new NotImplementedException();
    }
}
