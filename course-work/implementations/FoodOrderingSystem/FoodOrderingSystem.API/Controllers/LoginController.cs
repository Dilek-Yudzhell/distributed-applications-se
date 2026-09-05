using FoodOrderingSystem.API.Data;
using FoodOrderingSystem.API.DTOs;
using FoodOrderingSystem.API.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace FoodOrderingSystem.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class LoginController : ControllerBase
    {
        private readonly ApplicationDbContext _context;
        private readonly PasswordService _passwordService;
        private readonly IConfiguration _configuration;

        public LoginController(
            ApplicationDbContext context,
            PasswordService passwordService,
            IConfiguration configuration)
        {
            _context = context;
            _passwordService = passwordService;
            _configuration = configuration;
        }

        [HttpPost]
        [AllowAnonymous]
        public async Task<IActionResult> Login(
            [FromBody] LoginDto dto)
        {
            if (!ModelState.IsValid)
            {
                return ValidationProblem(ModelState);
            }

            var email = dto.Email.Trim();

            var user = await _context.Users
                .FirstOrDefaultAsync(u =>
                    u.Email.ToLower() == email.ToLower());

            if (user == null)
            {
                return Unauthorized(new
                {
                    message = "Invalid email or password."
                });
            }

            if (!user.IsActive)
            {
                return Unauthorized(new
                {
                    message = "User account is inactive."
                });
            }

            var passwordValid =
                _passwordService.VerifyPassword(
                    user.Password,
                    dto.Password);

            if (!passwordValid)
            {
                return Unauthorized(new
                {
                    message = "Invalid email or password."
                });
            }

            var claims = new[]
            {
                new Claim(
                    ClaimTypes.NameIdentifier,
                    user.Id.ToString()),

                new Claim(
                    ClaimTypes.Name,
                    user.FirstName),

                new Claim(
                    ClaimTypes.Email,
                    user.Email)
            };

            var jwtKey =
                _configuration["Jwt:Key"];

            var jwtIssuer =
                _configuration["Jwt:Issuer"];

            var jwtAudience =
                _configuration["Jwt:Audience"];

            if (string.IsNullOrWhiteSpace(jwtKey) ||
                string.IsNullOrWhiteSpace(jwtIssuer) ||
                string.IsNullOrWhiteSpace(jwtAudience))
            {
                return StatusCode(
                    StatusCodes.Status500InternalServerError,
                    new
                    {
                        message =
                            "JWT configuration is missing."
                    });
            }

            var key =
                new SymmetricSecurityKey(
                    Encoding.UTF8.GetBytes(jwtKey));

            var credentials =
                new SigningCredentials(
                    key,
                    SecurityAlgorithms.HmacSha256);

            var token =
                new JwtSecurityToken(
                    issuer: jwtIssuer,
                    audience: jwtAudience,
                    claims: claims,
                    expires:
                        DateTime.UtcNow.AddHours(2),
                    signingCredentials: credentials);

            var tokenString =
                new JwtSecurityTokenHandler()
                    .WriteToken(token);

            return Ok(new
            {
                message = "Login successful.",
                token = tokenString,
                userId = user.Id,
                firstName = user.FirstName,
                email = user.Email
            });
        }
    }
}