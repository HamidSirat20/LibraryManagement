using Asp.Versioning;
using LibraryManagement.WebAPI.Models.Dtos;
using LibraryManagement.WebAPI.Services.Interfaces;
using LibraryManagement.WebAPI.Services.ORM.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace LibraryManagement.WebAPI.Controllers
{
    [Route("api/v{version:ApiVersion}/[controller]")]
    [ApiVersion("1.0")]
    [ApiController]
    public class LateOrLostFeeController : ControllerBase
    {
        private readonly ILateReturnOrLostFeeService _lateReturnOrLostFeeService;
        private readonly ILateReturnOrLostFeeMapper _lateReturnOrLostFeeMapper;
        private readonly ICurrentUserService _currentUserService;
        public LateOrLostFeeController(ILateReturnOrLostFeeService lateReturnOrLostFeeService, ILateReturnOrLostFeeMapper lateReturnOrLostFeeMapper, ICurrentUserService currentUserService)
        {
            _lateReturnOrLostFeeService = lateReturnOrLostFeeService ?? throw new ArgumentNullException(nameof(lateReturnOrLostFeeService));
            _lateReturnOrLostFeeMapper = lateReturnOrLostFeeMapper ?? throw new ArgumentNullException(nameof(lateReturnOrLostFeeMapper));
            _currentUserService = currentUserService ?? throw new ArgumentNullException(nameof(currentUserService));
        }
        [HttpGet]
        public async Task<IActionResult> GetAllFees()
        {
            var fees = await _lateReturnOrLostFeeService.GetAllAsync();
            return Ok(fees);
        }

        [HttpGet("{fineId}", Name = "GetFineByIdAsync")]
        public async Task<IActionResult> GetFineByIdAsync([FromRoute] Guid fineId)
        {
            var fine = await _lateReturnOrLostFeeService.GetFineByIdAsync(fineId);
            return Ok(fine);
        }

        [HttpPost]
        public async Task<ActionResult<LateReturnOrLostFeeReadDto>> CreateLostFineAsync([FromBody] LostFineCreateDto lostFineCreateDto)
        {
            var createdFine = await _lateReturnOrLostFeeService.CreateLostFineAsync(lostFineCreateDto);
            var fineReadDto = _lateReturnOrLostFeeMapper.ToReadDto(createdFine);
            return CreatedAtAction(nameof(GetFineByIdAsync), new { fineId = fineReadDto.Id }, fineReadDto);
        }

        [HttpGet("my-fines")]
        public async Task<IActionResult> GetFineByIdForUserAsync()
        {
            var currentUserId = _currentUserService.UserId();

            var fine = await _lateReturnOrLostFeeService.GetFinesForUserAsync(currentUserId);
            return Ok(fine);

        }
    }
}
