using LibraryManagement.WebAPI.Services.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace LibraryManagement.WebAPI.Controllers;

[ApiController]
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

    }

