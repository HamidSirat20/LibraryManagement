using Asp.Versioning;
using LibraryManagement.WebAPI.Models;
using LibraryManagement.WebAPI.Models.Dtos;
using LibraryManagement.WebAPI.Services.Interfaces;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace LibraryManagement.WebAPI.Controllers;

[Route("api/v{version:ApiVersion}/[controller]")]
[ApiVersion("1.0")]
[ApiController]
public class AuthController : ControllerBase
{
    private readonly IUsersService _userService;
    private readonly IConfiguration _configuration;
    private readonly ILogger<AuthController> _logger;
    private readonly IPasswordService _passwordService;

    public AuthController(IUsersService userService, IConfiguration configuration, ILogger<AuthController> logger
, IPasswordService passwordService)
    {
        _userService = userService ?? throw new ArgumentNullException(nameof(userService));
        _configuration = configuration ?? throw new ArgumentNullException(nameof(configuration));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _passwordService = passwordService ?? throw new ArgumentNullException(nameof(passwordService));
    }
    [HttpPost()]
    public async Task<ActionResult<string>> Authenticate(LoginDto loginDto)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }
        var loginUser = await ValidateUser(loginDto);
        if (loginUser is null)
        {
            return Unauthorized("Invalid email or password.");
        }
        _logger.LogInformation($"User ID: {loginUser.Id}, Type: {loginUser.Id.GetType()}");

        //create a token
        var secretKey = _configuration["Authentication:SecretKey"];
        if (string.IsNullOrEmpty(secretKey))
        {
            throw new InvalidOperationException("JWT Secret Key is not configured");
        }
        var securityKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secretKey));
        var signingCredentials = new SigningCredentials(securityKey, SecurityAlgorithms.HmacSha256);

        //claims
        var claimsForToken = new List<Claim>();
        claimsForToken.Add(new Claim("sub", loginUser.Id.ToString()));
        claimsForToken.Add(new Claim("given_name", loginUser.FirstName));
        claimsForToken.Add(new Claim("family_name", loginUser.LastName));
        claimsForToken.Add(new Claim("email", loginUser.Email));
        claimsForToken.Add(new Claim("role", loginUser.Role.ToString()));

        //jwt 
        var jwtSecurityToken = new JwtSecurityToken(
            issuer: _configuration["Authentication:Issuer"],
            audience: _configuration["Authentication:Audience"],
            claims: claimsForToken,
            notBefore: DateTime.UtcNow,
            expires: DateTime.UtcNow.AddHours(1),
            signingCredentials: signingCredentials
            );


        var tokenToReturn = new JwtSecurityTokenHandler().WriteToken(jwtSecurityToken);
        return Ok(tokenToReturn);
    }

    private async Task<UserValidateDto?> ValidateUser(LoginDto loginDto)
    {
        try
        {
            var originalUser = await _userService.GetByEmailAsync(loginDto.Email);
            if (originalUser == null)
            {
                _logger.LogWarning("Authentication failed for email: {Email}. User not found.", loginDto.Email);
                return null;
            }

            // Add password verification
            if (!_passwordService.VerifyPassword(loginDto.Password, originalUser.Password, originalUser.Salt))
            {
                _logger.LogWarning("Authentication failed for email: {Email}. Invalid password.", loginDto.Email);
                return null;
            }

            return new UserValidateDto
            {
                Id = originalUser.Id,
                FirstName = originalUser.FirstName,
                LastName = originalUser.LastName,
                Email = originalUser.Email,
                Role = originalUser.Role
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "An error occurred while validating user with email: {Email}", loginDto.Email);
            throw;
        }
    }
    private class UserValidateDto
    {
        public Guid Id { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string Email { get; set; }
        public UserRole Role { get; set; }

    }

}

