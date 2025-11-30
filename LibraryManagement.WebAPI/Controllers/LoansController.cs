using Asp.Versioning;
using LibraryManagement.WebAPI.CustomExceptionHandler;
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
public class LoansController : ControllerBase
{
    private readonly ILogger<LoansController> _logger;
    private readonly ILoansService _loanService;
    private readonly ILoansMapper _loanMapper;
    private readonly ICurrentUserService _currentUserService;
    public LoansController(ILogger<LoansController> logger, ILoansService loanService, ILoansMapper loanMapper, ICurrentUserService currentUserService)
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

            var loanDto = await _loanService.MakeLoanAsync(loanCreateDto, userId);

            _logger.LogInformation("Successfully created loan with ID: {LoanId}", loanDto.Id);

            return CreatedAtAction("GetLoanById", new { id = loanDto.Id }, loanDto);
        }
        catch(BusinessRuleViolationException ex)
        {
            _logger.LogWarning(ex, "Business rule violation while making loan");
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
        var loans = await _loanService.GetAllLoansAsync(queryOptions);
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
    [HttpGet("{id}", Name = "GetLoanById")]
    public async Task<IActionResult> GetLoanById([FromRoute] Guid id)
    {
        try
        {
            var loan = await _loanService.GetLoanByIdAsync(id);
            if (loan is null)
            {
                _logger.LogError("There no such loan.");
                return BadRequest();
            }

            var loanDto = _loanMapper.ToLoanReadDto(loan);
            return Ok(loanDto);
        }
        catch (BusinessRuleViolationException ex)
        {
            var statusCode = ex.ErrorCode switch
            {
                "Not_Found" => StatusCodes.Status404NotFound,
                _ => StatusCodes.Status400BadRequest
            };

            return Problem(
                       detail: ex.Message,
                       title: "Loan not found",
                       statusCode: statusCode,
                       type: $"https://localhost:7127//{ex.ErrorCode}");
        }
        catch
        {
            throw;
        }
    }

    [HttpPatch("{loanId}")]
    public async Task<IActionResult> ReturnLoanAsync([FromRoute] Guid loanId)
    {
        try
        {
            var loan = await _loanService.ReturnBookAsync(loanId);
            var loanDto = _loanMapper.ToLoanReadDto(loan);
            return Ok(loanDto);
        }
        catch (BusinessRuleViolationException ex)
        {
            _logger.LogWarning(ex, "Failed to return loan with id {LoanId}", loanId);
            return Problem(
                detail: "Cannot return this loan.",
                title: "Return Loan Error",
                statusCode: ex.ErrorCode == "Not_Found" ? 404 : 400
            );
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unexpected error while returning loan with id {LoanId}", loanId);
            return StatusCode(500, "An unexpected error occurred.");
        }
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
        var loans = await _loanService.GetYourOwnLoansAsync(userId, includeReturnedLoans);
        if (!loans.Any())
        {
            _logger.LogInformation("No active loan found for the current user.");
            return NotFound("You have no loaned books.");
        }
        var loanDtos = loans.Select(l => _loanMapper.ToLoanReadDto(l));
        return Ok(loanDtos);
    }

    [HttpGet("user/{userId}")]
    public async Task<IActionResult> GetLoansByUserIdAsync([FromRoute] Guid userId, [FromQuery] bool includeReturnedLoans = false)
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

    [HttpPut("{loanId}")]
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

