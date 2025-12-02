using Asp.Versioning;
using LibraryManagement.WebAPI.Models.Common;
using LibraryManagement.WebAPI.Models.Dtos;
using LibraryManagement.WebAPI.Services.Interfaces;
using LibraryManagement.WebAPI.Services.ORM.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Text.Json;
using static LibraryManagement.WebAPI.Helpers.ApiRoutes;

namespace LibraryManagement.WebAPI.Controllers;

[ApiController]
//[Authorize]
[Route("api/v{version:ApiVersion}/[controller]")]
[ApiVersion("1.0")]
public class UsersController : ControllerBase
{
    private readonly IUsersService _userService;
    private readonly IUsersMapper _userMapper;

    public UsersController(IUsersService userService, IUsersMapper userMapper)
    {
        _userService = userService ?? throw new ArgumentNullException(nameof(userService));
        _userMapper = userMapper ?? throw new ArgumentNullException(nameof(userMapper));
    }
    [HttpGet]
    [HttpHead]
    //[Authorize(Policy = "AdminCanAccess")]
    public async Task<IActionResult> ListALLUsers([FromQuery] QueryOptions queryOptions)
    {
        var users = await _userService.ListAllUsersAsync(queryOptions);

        if (users == null)
        {
            return NotFound();
        }
        var userDtos = users.Select(user => _userMapper.ToReadDto(user));
        // paginationMetadata 
        var paginationMetadata = new
        {
            totalCount = users.TotalRecords,
            pageSize = users.PageSize,
            currentPage = users.CurrentPage,
            totalPages = users.TotalPages,
        };

        Response.Headers["X-Pagination"] = JsonSerializer.Serialize(paginationMetadata);

        return Ok(userDtos);
    }

    [HttpGet("{id}", Name = "GetOneUser")]
    public async Task<IActionResult> GetOneUser(Guid id)
    {
        var user = await _userService.GetByIdAsync(id);
        if (user == null)
        {
            return NotFound();
        }
        var userReadDto = _userMapper.ToReadDto(user);
        return Ok(userReadDto);
    }

    [HttpDelete("{id}")]
    //[Authorize(Policy = "AdminCanAccess")]
    public async Task<IActionResult> DeleteOneUser(Guid id)
    {
        var exists = await _userService.EntityExistAsync(id);
        if (!exists)
        {
            return NotFound();
        }
        await _userService.DeleteByIdAsync(id);
        return NoContent();
    }

    [HttpPost]
    public async Task<IActionResult> CreateOneUser([FromForm] UserCreateDto userCreateDto)
    {
        if (userCreateDto == null)
        {
            return BadRequest("User data is null");
        }

        var createdUser = await _userService.CreateUserAsync(userCreateDto);

        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }
        if (createdUser == null)
        {
            return BadRequest("User is empty");
        }
        var userReadDto = _userMapper.ToReadDto(createdUser);
        return CreatedAtRoute(
                "GetOneUser",
                 new { id = userReadDto.Id },
                 userReadDto);
    }
    [HttpPost("admin")]
    //[Authorize(Policy = "AdminCanAccess")]
    public async Task<IActionResult> CreateAdmin([FromForm] UserCreateDto userCreateDto)
    {
        if (userCreateDto == null)
        {
            return BadRequest("User data is null");
        }
        var createdUser = await _userService.CreateAdminAsync(userCreateDto);
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }
        if (createdUser == null)
        {
            return BadRequest("User is empty");
        }
        var userReadDto = _userMapper.ToReadDto(createdUser);
        return CreatedAtRoute("GetOneUser", new { id = createdUser.Id }, userReadDto);
    }
    [HttpGet("by-email/{email}")]
    // [Authorize(Policy = "AdminCanAccess")]
    public async Task<ActionResult<UserReadDto>> GetUserByEmail(string email)
    {
        var user = await _userService.GetByEmailAsync(email);
        if (user == null)
        {
            return NotFound();
        }
        return Ok(user);
    }
    [HttpGet("with-active-loans")]
    // [Authorize(Policy = "AdminCanAccess")]
    public async Task<IActionResult> GetUserWithActiveLoan()
    {
        var users = await _userService.GetUsersWithActiveLoansAsync();
        return Ok(users);
    }
    [HttpGet("with-overdue-loans")]
    public async Task<IActionResult> GetUserWithOverdueLoan()
    {
        var users = await _userService.GetUsersWithOverdueLoansAsync();
        return Ok(users);
    }
    [HttpPut("{id}")]
    [Authorize(Policy = "AdminCanAccess")]
    public async Task<IActionResult> UpdateUser(Guid id, [FromBody] UserUpdateDto userUpdateDto)
    {
        if (userUpdateDto == null)
        {
            return BadRequest("User data is null");
        }
        var updatedUser = await _userService.UpdateUserAsync(id, userUpdateDto);
        if (updatedUser is null)
        {
            return NotFound();
        }
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }
        return Ok(updatedUser);
    }
    [HttpPost("extend-membership")]
    [Authorize(Policy = "AdminCanAccess")]
    public async Task<IActionResult> ExtendUserMembership( [FromBody] Guid id)
    {
      
        var extendedUser = await _userService.ExtendUserMembership(id);

        return Ok(extendedUser);
    }
    [HttpOptions()]
    public IActionResult GetUsersOptions()
    {
        Response.Headers.Add("Allow", "GET,HEAD,OPTIONS,POST");
        return Ok();
    }
    [HttpOptions("{id}")]
    public IActionResult GetUsersOptionWithId(Guid id)
    {
        Response.Headers.Add("Allow", "GET,PATCH,PUT");
        return Ok();
    }

}

