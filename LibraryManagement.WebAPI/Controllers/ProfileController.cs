using Asp.Versioning;
using LibraryManagement.WebAPI.Data;
using LibraryManagement.WebAPI.Services.ORM;
using LibraryManagement.WebAPI.Services.ORM.Interfaces;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace LibraryManagement.WebAPI.Controllers;

[Route("api/v{version:ApiVersion}/users/[controller]")]
[ApiVersion("1.0")]
[ApiController]
public class ProfileController : ControllerBase
{
    private readonly LibraryDbContext _context;
    private readonly IUsersMapper _userMapper;

    public ProfileController(LibraryDbContext context, IUsersMapper userMapper)
    {
        _context = context ?? throw new ArgumentNullException(nameof(context));
        _userMapper = userMapper;
    }
    [HttpGet]
    public async Task<IActionResult> GetUserProfile()
    {
        var userIdString = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

        if (string.IsNullOrEmpty(userIdString))
        {
            return Unauthorized("User ID not found in claims");
        }

        if (!Guid.TryParse(userIdString, out Guid userId))
        {
            return BadRequest("Invalid user ID format");
        }

        var user = await _context.Users.FirstOrDefaultAsync(u => u.Id.ToString() == userIdString);

        if (user == null)
        {
            return NotFound("User not found");
        }
        return Ok(_userMapper.ToReadDto(user));
    }

}
