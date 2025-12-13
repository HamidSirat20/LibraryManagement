using LibraryManagement.Test.Fixtures;
using LibraryManagement.WebAPI.Data;
using LibraryManagement.WebAPI.Services.Implementations;

namespace LibraryManagement.Test.ServiceTests;
public class LateReturnOrLostFeeServiceTests : IClassFixture<LateReturnOrLostFeeServiceFixture>
{
    private readonly LibraryDbContext _dbContext;
    private readonly LateReturnOrLostFeeServiceFixture _fixture;
    private readonly LateReturnOrLostFeeService _service;
    public LateReturnOrLostFeeServiceTests()
    {
        _dbContext = _fixture.DbContext ?? throw new ArgumentNullException(nameof(LateReturnOrLostFeeServiceFixture));
        _service = _fixture.LateReturnOrLostFee;

    }

}
