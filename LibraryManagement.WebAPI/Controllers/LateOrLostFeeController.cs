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
        public LateOrLostFeeController(ILateReturnOrLostFeeService lateReturnOrLostFeeService, ILateReturnOrLostFeeMapper lateReturnOrLostFeeMapper)
        {
            _lateReturnOrLostFeeService = lateReturnOrLostFeeService ?? throw new ArgumentNullException(nameof(lateReturnOrLostFeeService));
            _lateReturnOrLostFeeMapper = lateReturnOrLostFeeMapper ?? throw new ArgumentNullException(nameof(lateReturnOrLostFeeMapper));
        }
        // GET: api/<LateOrLostFeeController>
        [HttpGet]
        public async Task<IActionResult> GetAllFees()
        {
           var fees = await _lateReturnOrLostFeeService.GetAllAsync();
            return Ok(fees);
        }

        [HttpGet("{id}",Name = "GetFineByIdAsync")]
        public async Task<IActionResult> GetFineByIdAsync(Guid fineId)
        {
            try
            {
                var fine = _lateReturnOrLostFeeService.GetFineByIdAsync(fineId);
                return Ok(fine);

            }
            catch(Exception ex)
            {
                throw;
            }
        }

        [HttpPost]
        public async Task<ActionResult<LateReturnOrLostFeeReadDto>> CreateLostFineAsync([FromBody] LostFineCreateDto lostFineCreateDto)
        {
            var createdFine = await _lateReturnOrLostFeeService.CreateLostFineAsync(lostFineCreateDto);
            var fineReadDto = _lateReturnOrLostFeeMapper.ToReadDto(createdFine);
            return CreatedAtAction(nameof(GetFineByIdAsync), new { id = fineReadDto.Id }, fineReadDto);
        }

        // PUT api/<LateOrLostFeeController>/5
        [HttpPut("{id}")]
        public void Put(int id, [FromBody] string value)
        {
        }

        // DELETE api/<LateOrLostFeeController>/5
        [HttpDelete("{id}")]
        public void Delete(int id)
        {
        }
    }
}
