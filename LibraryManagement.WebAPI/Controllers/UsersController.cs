using LibraryManagement.WebAPI.Models.Dtos;
using LibraryManagement.WebAPI.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace LibraryManagement.WebAPI.Controllers;

[ApiController]
[Authorize]
[Route("api/v1/[controller]")]
    public class UsersController : ControllerBase
{
    private readonly IUserService _userService;

    public UsersController( IUserService userService)
    {
        _userService = userService;
    }
    [HttpGet]
    public async Task<IActionResult> ListALLUsers()
    {
       var users = await _userService.ListAllUsersAsync();
        return Ok(users);
    }
    [HttpGet("{id}")]
    public async Task<IActionResult> GetOneUser(Guid id)
    {
        var bookId = HttpContext.Request.RouteValues["id"];
        Console.WriteLine(bookId);
       var user = await _userService.GetByIdAsync(id);
        if(user == null)
        {
            return NotFound();
        }
        return Ok(user);
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteOneUser(Guid id)
    {
        var exists = await _userService.EntityExistAsync(id);
        if(!exists)
        {
            return NotFound();
        }
        await _userService.DeleteByIdAsync(id);
        return NoContent();
    }

    [HttpPost]
    public async Task<IActionResult> CreateOneUser([FromBody] UserCreateDto userCreateDto)
    {
        if(userCreateDto == null)
        {
            return BadRequest("User data is null");
        }
        var createdUser = await _userService.CreateUserAsync(userCreateDto);
        if(!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }
        if (createdUser == null)
        {
            return BadRequest("User is empty");
        }
        return CreatedAtAction(nameof(GetOneUser), new { id = createdUser.Id }, createdUser);
    }
    [HttpPost("admin")]
    public async Task<IActionResult> CreateAdmin([FromBody] UserCreateDto userCreateDto)
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
        return CreatedAtAction(nameof(GetOneUser), new { id = createdUser.Id }, createdUser);
    }
    [HttpGet("by-email/{email}")]
    public async Task<ActionResult<UserReadDto>> GetUserByEmial(string email)
    {
        var user = await _userService.GetByEmailAsync(email);
        if (user == null)
        {
            return NotFound();
        }
        return Ok(user);
    }
    [HttpGet("with-active-loans")]
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

}

