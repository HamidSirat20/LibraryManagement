using Asp.Versioning;
using LibraryManagement.WebAPI.CustomExceptionHandler;
using LibraryManagement.WebAPI.Models.Common;
using LibraryManagement.WebAPI.Services.Interfaces;
using LibraryManagement.WebAPI.Services.ORM.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Infrastructure;

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
            var (statusCode, detailResult) = ex.ErrorCode switch
            {
                "BOOK_NOT_FOUND" => (StatusCodes.Status404NotFound,(IActionResult?)NotFound(ex.Message)),
                "BOOK_AVAILABLE" => ( StatusCodes.Status409Conflict,(IActionResult?)Conflict(ex.Message)),
                "DUPLICATE_RESERVATION" => (StatusCodes.Status400BadRequest,(IActionResult?)BadRequest(ex.Message)),
                _ => (StatusCodes.Status500InternalServerError, (IActionResult?) BadRequest(ex.Message))
            };

            return Problem(
                         detail: ex.Message,
                         title: "Reservation Error",
                         statusCode: statusCode,
                         type: $"https://localhost:7127//{ex.ErrorCode}" );
        }
        catch (Exception ex) when (ex is not BusinessRuleViolationException)
        {
            _logger.LogError(ex, "Unexpected error in MakeReservation endpoint.");
            return StatusCode(StatusCodes.Status500InternalServerError, new
            {
                error = "An unexpected error occurred. Please try again."
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
            var (statusCode, detailResult) = ex.ErrorCode switch
            {
                "RESERVATION_NOT_FOUND" => (StatusCodes.Status404NotFound, (IActionResult?)NotFound(ex.Message)),
                "UNAUTHORIZED_PICKUP" => (StatusCodes.Status401Unauthorized, (IActionResult?)Unauthorized(ex.Message)),
                "INVALID_RESERVATION_STATUS" => (StatusCodes.Status409Conflict, (IActionResult?)Conflict(ex.Message)),
                "BOOK_NOT_AVAILABLE" => (StatusCodes.Status409Conflict, (IActionResult?)Conflict(ex.Message)),
                _ => (StatusCodes.Status500InternalServerError, (IActionResult?)StatusCode(500, new { error = "An unexpected error occurred" }))
            };

            if (detailResult != null)
                return detailResult;

            return Problem(
                detail: ex.Message,
                title: "Pickup Error",
                statusCode: statusCode,
                type: $"https://localhost:7127/{ex.ErrorCode}");
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

    [HttpGet]
    public async Task<IActionResult> ListAllReservations(
        [FromQuery] QueryOptions queryOptions)
    {
        var reservations = await _reservationService.ListAllReservationAsync(queryOptions);

        var reservationDtos = reservations.Select(r => _reservationMapper.ToReservationReadDto(r));
        return Ok(reservationDtos);
    }

    [HttpPatch("{reservationId}")]
    public async Task<IActionResult> CancelReservationAsync([FromRoute] Guid reservationId)
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
                _logger.LogWarning("Unauthorized access attempt. Missing user ID.");
                return Unauthorized(problem);
            }

            var reservation = await _reservationService.CancelReservationAsync(reservationId, userId);
            var reservationDto = _reservationMapper.ToReservationReadDto(reservation);
            return Ok(reservationDto);
        }
        catch (BusinessRuleViolationException ex)
        {
            var (statusCode, detailResult) = ex.ErrorCode switch
            {
                "NOT_FOUND" => (StatusCodes.Status404NotFound, (IActionResult?)NotFound(ex.Message)),
                "UNAUTHORIZED_CANCEL" => (StatusCodes.Status401Unauthorized, (IActionResult?)Unauthorized(ex.Message)),
                _ => (StatusCodes.Status500InternalServerError, (IActionResult?)StatusCode(500, new { error = "An unexpected error occurred" }))
            };
            if (detailResult != null)
                return detailResult;
            return Problem(
                detail: ex.Message,
                title: "Cancellation Error",
                statusCode: statusCode,
                type: $"https://localhost:7127/{ex.ErrorCode}");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unexpected error in CancelReservationAsync endpoint.");
            return StatusCode(StatusCodes.Status500InternalServerError, new
            {
                error = "An unexpected error occurred. Please try again."
            });
        }
    }

    [HttpGet("my-reservation")]
    public async Task<IActionResult> GetReservationsByUserId()
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
                _logger.LogWarning("Unauthorized access attempt. Missing user ID.");
                return Unauthorized(problem);
            }

            var reservations = await _reservationService.ListReservationForAUserAsync(userId);
            var reservationDtos = reservations.Select(r => _reservationMapper.ToReservationReadDto(r));
            return Ok(reservationDtos);

        }
        catch (BusinessRuleViolationException ex)
        {
            var (statusCode, detailResult) = ex.ErrorCode switch
            {
                "NO_RESERVATIONS" => (StatusCodes.Status404NotFound, (IActionResult?)NotFound(ex.Message)),
                _ => (StatusCodes.Status500InternalServerError, (IActionResult?)StatusCode(500, new { error = "An unexpected error occurred" }))
            };
            if (detailResult != null)
                return detailResult;
            return Problem(
                detail: ex.Message,
                title: "Get Reservations Error",
                statusCode: statusCode,
                type: $"https://localhost:7127/{ex.ErrorCode}");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unexpected error in GetReservationsByUserId endpoint.");
            return StatusCode(StatusCodes.Status500InternalServerError, new
            {
                error = "An unexpected error occurred. Please try again."
            });
        }
    }

}

