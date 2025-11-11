using Asp.Versioning;
using LibraryManagement.WebAPI.Models.Common;
using LibraryManagement.WebAPI.Models.Dtos;
using LibraryManagement.WebAPI.Services.Interfaces;
using LibraryManagement.WebAPI.Services.ORM.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace LibraryManagement.WebAPI.Controllers;

    [Route("api/v{version:ApiVersion}/[controller]")]
    [ApiController]
    [ApiVersion("1.0")]
    [Authorize]
public class LoanController : ControllerBase
    {
        private readonly ILogger<LoanController> _logger;
        private readonly ILoanService _loanService;
        private readonly ILoanMapper _loanMapper;
    private readonly ICurrentUserService _currentUserService;
    public LoanController(ILogger<LoanController> logger, ILoanService loanService, ILoanMapper loanMapper, ICurrentUserService currentUserService)
    {
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _loanService = loanService ?? throw new ArgumentNullException(nameof(loanService));
        _loanMapper = loanMapper ?? throw new ArgumentNullException(nameof(loanMapper));
        _currentUserService = currentUserService ?? throw new ArgumentNullException(nameof(currentUserService));
    }

    [HttpPost(Name = "MakeLoanAsync")]
    public async Task<IActionResult> MakeLoanAsync([FromBody] LoanCreateDto loanCreateDto)
    {
        try
        {
            // Fetch current logged in user
            var userId = _currentUserService.UserId();

            var loan = await _loanService.MakeLoanAsync(new LoanCreateDto
            {
                BookId = loanCreateDto.BookId,
                UserId = userId,
                LoanDate = DateTime.UtcNow,
                DueDate = DateTime.UtcNow.AddDays(30),
                ReturnDate = null
            });

            var loanDto = _loanMapper.ToLoanReadDto(loan);
            _logger.LogInformation("Successfully created loan with ID: {LoanId}", loan.Id);

            return CreatedAtAction("GetLoanById", new { id = loan.Id }, loanDto);
        }
        catch (KeyNotFoundException ex)
        {
            _logger.LogWarning(ex, "Book not found");
            return NotFound(new { error = ex.Message });
        }
        catch (InvalidOperationException ex)
        {
            _logger.LogWarning(ex, "Invalid operation during loan creation");
            return BadRequest(new { error = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unexpected error occurred while making loan");
            return StatusCode(500, new { error = "An unexpected error occurred. Please try again later." });
        }
    }
    [HttpGet]
    public async Task<IActionResult> ListAllLoansAsync([FromQuery] QueryOptions queryOptions)
    {
        var loans = await  _loanService.GetAllLoansAsync(queryOptions);
        if (!loans.Any())
        {
            _logger.LogInformation("No active loan found with the provided query options.");
            return NotFound("No loan found.");
        }
        //  paginationMetadata
        var paginationMetadata = new
        {
            TotalCount = loans.TotalRecords,
            PageSize = loans.PageSize,
            CurrentPage = loans.CurrentPage,
            TotalPages = loans.TotalPages,
            HasNext = loans.HasNext,
            HasPrevious = loans.HasPrevious
        };
        Response.Headers.Add("X-Pagination", System.Text.Json.JsonSerializer.Serialize(paginationMetadata));

        var loanDtos = loans.Select(loan => _loanMapper.ToLoanReadDto(loan)).ToList();
        return Ok(loanDtos);
    }
    [HttpGet("{id}",Name ="GetLoanById")]
    public async Task<IActionResult> GetLoanById([FromRoute] Guid id)
    {
        var loan =await _loanService.GetLoanByIdAsync(id);
        if(loan is null)
        {
            _logger.LogError("There no such loan.");
            return BadRequest();
        }
        var loanDto =  _loanMapper.ToLoanReadDto(loan);
        return Ok(loanDto);
    }

    [HttpPut("{loanId}")]
    public async Task<IActionResult> ReturnLoanAsync([FromRoute] Guid loanId)
    {
        var returnLoan =await _loanService.ReturnBookAsync(loanId);
        if(returnLoan is null)
            {
            _logger.LogError("There no such loan to return.");
            return BadRequest();
        }
        var loanDto = _loanMapper.ToLoanReadDto(returnLoan);
        return Ok(loanDto);
    }

    [HttpGet("my-loans")]
    public async Task<IActionResult> GetYourOwnLoansAsync([FromQuery] bool includeReturnedLoans = false)
    {
        // Fetch current logged in user
        var userId = _currentUserService.UserId();

        if (string.IsNullOrEmpty(userId.ToString()))
        {
            _logger.LogError("You need to log in in order to see you loans.");
            return Unauthorized("You need to log in in order to see you loans.");
        }
        var loans = await _loanService.GetYourOwnLoansAsync(userId,includeReturnedLoans);
        if (!loans.Any())
        {
            _logger.LogInformation("No active loan found for the current user.");
            return NotFound("You have no loaned books.");
        }
        var loanDtos = loans.Select(l => _loanMapper.ToLoanReadDto(l));
        return Ok(loanDtos);
    }

    [HttpGet("user/{userId}")]
    public async Task<IActionResult> GetLoansByUserIdAsync([FromRoute] Guid userId,[FromQuery] bool includeReturnedLoans = false)
    {
        var loans = await _loanService.GetYourOwnLoansAsync(userId, includeReturnedLoans);
        if (!loans.Any())
        {
            _logger.LogInformation("No active loan found for the specified user.");
            return NotFound("The user has no loaned books.");
        }
        var loanDtos = loans.Select(l => _loanMapper.ToLoanReadDto(l));
        return Ok(loanDtos);
    }

    [HttpGet("overdue")]
    public async Task<IActionResult> GetOverdueLoansAsync()
    {
        var loans = await _loanService.GetOverdueLoansAsync();
        if (!loans.Any())
        {
            _logger.LogInformation("No overdue loans found.");
            return NotFound("There are no overdue loans.");
        }
        var loanDtos = loans.Select(l => _loanMapper.ToLoanReadDto(l));
        return Ok(loanDtos);
    }

    [HttpPut("update/{loanId}")]
    public async Task<IActionResult> UpdateLoanAsync([FromRoute] Guid loanId)
    {
        var updatedLoan = await _loanService.UpdateLoanAsync(loanId);
        if (updatedLoan is null)
        {
            _logger.LogError("There no such loan to update.");
            return BadRequest();
        }
        var loanDto = _loanMapper.ToLoanReadDto(updatedLoan);
        return Ok(loanDto);
    }
}

