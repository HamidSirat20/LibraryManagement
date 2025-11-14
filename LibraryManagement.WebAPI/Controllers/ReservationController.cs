using Asp.Versioning;
using LibraryManagement.WebAPI.CustomExceptionHandler;
using LibraryManagement.WebAPI.Services.Interfaces;
using LibraryManagement.WebAPI.Services.ORM.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Infrastructure;
using Microsoft.EntityFrameworkCore;

namespace LibraryManagement.WebAPI.Controllers;

[ApiController]
[Route("api/v{version:ApiVersion}/[controller]")]
[ApiVersion("1.0")]
[Authorize]
public class ReservationController : ControllerBase
{
    private readonly IReservationService _reservationService;
    private readonly ILogger<ReservationController> _logger;
    private readonly IReservationMapper _reservationMapper;
    private readonly ICurrentUserService _currentUserService;
    private readonly ProblemDetailsFactory _problemDetailsFactory;
    public ReservationController(IReservationService reservationService, ILogger<ReservationController> logger, IReservationMapper reservationMapper, ICurrentUserService currentUserService, ProblemDetailsFactory problemDetial)
    {
        _reservationService = reservationService ?? throw new ArgumentNullException(nameof(reservationService));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _reservationMapper = reservationMapper ?? throw new ArgumentNullException(nameof(reservationMapper));
        _currentUserService = currentUserService ?? throw new ArgumentNullException(nameof(currentUserService));
        _problemDetailsFactory = problemDetial ?? throw new ArgumentNullException(nameof(problemDetial));
    }

    [HttpPost("{bookId}")]
    public async Task<IActionResult> MakeReservation([FromRoute] Guid bookId)
    {
        try
        {
            var userId = _currentUserService.UserId();
            if (userId == Guid.Empty )
            {
                var problem = _problemDetailsFactory.CreateProblemDetails(
                              HttpContext,
                              statusCode: StatusCodes.Status401Unauthorized,
                              title: "Unauthorized Access",                                                                        
                              detail: "User ID could not be determined from the current session.",
                              instance: HttpContext.Request.Path);

                _logger.LogWarning("Unauthorized access attempt. Missing user ID.");

                return Unauthorized(problem);
            }

            var reservationReadDto = await _reservationService.CreateReservationAsync(bookId, userId);
            return Ok(reservationReadDto);
        }
        catch (BusinessRuleViolationException ex)
        {
            var statusCode = ex.ErrorCode switch
            {
                "BOOK_NOT_FOUND" => StatusCodes.Status404NotFound,
                "BOOK_AVAILABLE" => StatusCodes.Status409Conflict,
                "DUPLICATE_RESERVATION" => StatusCodes.Status400BadRequest,
                "UNEXPECTED_ERROR"=>StatusCodes.Status409Conflict,
                _ => StatusCodes.Status400BadRequest
            };

            return Problem(
                         detail: ex.Message,
                         title: "Reservation Error",
                         statusCode: statusCode,
                         type: $"https://localhost:7127//{ex.ErrorCode}" );
        }
        catch (DbUpdateException ex)
        {
            _logger.LogError(ex, "Database error in MakeReservation endpoint");
            return StatusCode(StatusCodes.Status500InternalServerError, new
            {
                error = "A system error occurred. Please try again."
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unexpected error in MakeReservation endpoint");
            return StatusCode(StatusCodes.Status500InternalServerError, new
            {
                error = "An unexpected error occurred. Please try again.fff"
            });
        }
    }
    [HttpPost("pickup/{reservationId}")]
    public async Task<IActionResult> PickUpReservation([FromRoute] Guid reservationId)
    {
        try
        {
            var userId = _currentUserService.UserId();

            if (userId == Guid.Empty)
            {
                var problem = _problemDetailsFactory.CreateProblemDetails(
                    HttpContext,
                    statusCode: StatusCodes.Status401Unauthorized,
                    title: "Unauthorized Access",
                    detail: "User ID could not be determined from the current session.",
                    instance: HttpContext.Request.Path);

                _logger.LogWarning("Unauthorized access attempt during pickup. Missing user ID.");

                return Unauthorized(problem);
            }

            var pickupDto = await _reservationService.PickReservationByIdAsync(reservationId, userId);
            return Ok(pickupDto);
        }
        catch (BusinessRuleViolationException ex)
        {
            var statusCode = ex.ErrorCode switch
            {
                "RESERVATION_NOT_FOUND" => StatusCodes.Status404NotFound,
                "No_Reservation" => StatusCodes.Status409Conflict,
                "UNAUTHORIZED_PICKUP" => StatusCodes.Status403Forbidden,
                _ => StatusCodes.Status400BadRequest
            };

            return Problem(
                detail: ex.Message,
                title: "Pickup Error",
                statusCode: statusCode,
                type: $"https://localhost:7127/{ex.ErrorCode}");
        }
        catch (DbUpdateException ex)
        {
            _logger.LogError(ex, "Database error in PickUpReservation endpoint");
            return StatusCode(StatusCodes.Status500InternalServerError, new
            {
                error = "A system error occurred while processing your pickup request."
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unexpected error in PickUpReservation endpoint");
            return StatusCode(StatusCodes.Status500InternalServerError, new
            {
                error = "An unexpected error occurred. Please try again."
            });
        }
    }

}

