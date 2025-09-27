using LibraryManagement.WebAPI.Models.Dtos;
using LibraryManagement.WebAPI.Services.Interfaces;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace LibraryManagement.WebAPI.Controllers
{
    [Route("api/v1/[controller]")]
    [ApiController]
    public class AuthController : ControllerBase
    {
        private readonly IUserService _userService;
        private readonly IConfiguration _configuration;

        public AuthController(IUserService userService, IConfiguration configuration
            )
        {
            _userService = userService ?? throw new ArgumentNullException(nameof(userService));
            _configuration = configuration ?? throw new ArgumentNullException(nameof(configuration));
        }
        [HttpPost("authenticate")]
        public ActionResult<string> Authenticate(LoginDto loginDto)
        {
            var loginUser = ValidateUser(loginDto);
            if (loginUser is null)
            {
                return Unauthorized();
            }
            //create a token
            var securityKey =new SymmetricSecurityKey(Encoding.Unicode.GetBytes(_configuration["Authentication:secretKey"]));
            var signingCredentials = new SigningCredentials(securityKey, SecurityAlgorithms.HmacSha256);

            //claims
            var claimsForToken = new List<Claim>();
            claimsForToken.Add(new Claim("sub", loginUser.Id.ToString()));
            claimsForToken.Add(new Claim("given_name", loginUser.Result.FirstName ));
            claimsForToken.Add(new Claim("family_name", loginUser.Result.LastName));
            claimsForToken.Add(new Claim("email", loginUser.Result.Email));
           
            //jwt 
            var jwtSecurityToken  = new JwtSecurityToken(
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
        private async Task< UserValidateDto> ValidateUser(LoginDto loginDto)
        {
            var user = await _userService.GetByEmailAsync(loginDto.Email);
            if (user == null)
                return null;
            return new UserValidateDto
                {
                    Id = user.Id,
                    FirstName = user.FirstName,
                    LastName = user.LastName,
                    Email = user.Email,
                };
        }


        private class UserValidateDto
        {
            public Guid Id { get; set; }
            public string FirstName { get; set; }
            public string LastName { get; set; }
            public string Email { get; set; }

        }

    }
}
