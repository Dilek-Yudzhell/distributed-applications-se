using FoodOrderingSystem.API.Data;
using FoodOrderingSystem.API.DTOs;
using FoodOrderingSystem.API.Models;
using FoodOrderingSystem.API.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace FoodOrderingSystem.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class UsersController : ControllerBase
    {
        private readonly ApplicationDbContext _context;
        private readonly PasswordService _passwordService;

        public UsersController(
            ApplicationDbContext context,
            PasswordService passwordService)
        {
            _context = context;
            _passwordService = passwordService;
        }

        // =========================================================
        // GET: api/Users
        // Search + Pagination + Sorting
        // =========================================================

        [HttpGet]
        public async Task<IActionResult> GetUsers(
            string? firstName = null,
            string? lastName = null,
            int page = 1,
            int pageSize = 10,
            string sortBy = "id")
        {
            if (page < 1)
            {
                return BadRequest("Page must be greater than 0.");
            }

            if (pageSize < 1 || pageSize > 100)
            {
                return BadRequest(
                    "Page size must be between 1 and 100.");
            }

            var query = _context.Users
                .AsNoTracking()
                .AsQueryable();

            // Search by first name
            if (!string.IsNullOrWhiteSpace(firstName))
            {
                query = query.Where(u =>
                    u.FirstName.Contains(firstName));
            }

            // Search by last name
            if (!string.IsNullOrWhiteSpace(lastName))
            {
                query = query.Where(u =>
                    u.LastName.Contains(lastName));
            }

            // Sorting
            query = sortBy.ToLower() switch
            {
                "firstname" =>
                    query.OrderBy(u => u.FirstName),

                "lastname" =>
                    query.OrderBy(u => u.LastName),

                "email" =>
                    query.OrderBy(u => u.Email),

                "registrationdate" =>
                    query.OrderBy(u => u.RegistrationDate),

                "id" =>
                    query.OrderBy(u => u.Id),

                _ =>
                    throw new ArgumentException(
                        "Invalid sortBy value.")
            };

            var totalCount = await query.CountAsync();

            var users = await query
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .Select(u => new UserDto
                {
                    Id = u.Id,
                    FirstName = u.FirstName,
                    LastName = u.LastName,
                    Email = u.Email,
                    Phone = u.Phone,
                    RegistrationDate = u.RegistrationDate,
                    IsActive = u.IsActive
                })
                .ToListAsync();

            return Ok(new
            {
                page,
                pageSize,
                totalCount,
                hasNextPage =
                    page * pageSize < totalCount,
                data = users
            });
        }

        // =========================================================
        // GET: api/Users/5
        // =========================================================

        [HttpGet("{id}")]
        public async Task<IActionResult> GetUser(int id)
        {
            var user = await _context.Users
                .AsNoTracking()
                .Where(u => u.Id == id)
                .Select(u => new UserDto
                {
                    Id = u.Id,
                    FirstName = u.FirstName,
                    LastName = u.LastName,
                    Email = u.Email,
                    Phone = u.Phone,
                    RegistrationDate = u.RegistrationDate,
                    IsActive = u.IsActive
                })
                .FirstOrDefaultAsync();

            if (user == null)
            {
                return NotFound(
                    new
                    {
                        message = "User not found."
                    });
            }

            return Ok(user);
        }

        // =========================================================
        // POST: api/Users
        // Registration does not require JWT
        // =========================================================

        [HttpPost]
        [AllowAnonymous]
        public async Task<IActionResult> CreateUser(
            [FromBody] CreateUserDto dto)
        {
            if (!ModelState.IsValid)
            {
                return ValidationProblem(ModelState);
            }

            var emailExists = await _context.Users
                .AnyAsync(u => u.Email == dto.Email);

            if (emailExists)
            {
                return BadRequest(
                    new
                    {
                        message = "Email already exists."
                    });
            }

            var user = new User
            {
                FirstName = dto.FirstName.Trim(),
                LastName = dto.LastName.Trim(),
                Email = dto.Email.Trim(),
                Password = _passwordService
                    .HashPassword(dto.Password),
                Phone = dto.Phone.Trim(),
                RegistrationDate = DateTime.UtcNow,
                IsActive = true
            };

            _context.Users.Add(user);

            await _context.SaveChangesAsync();

            var result = new UserDto
            {
                Id = user.Id,
                FirstName = user.FirstName,
                LastName = user.LastName,
                Email = user.Email,
                Phone = user.Phone,
                RegistrationDate =
                    user.RegistrationDate,
                IsActive = user.IsActive
            };

            return CreatedAtAction(
                nameof(GetUser),
                new { id = user.Id },
                result);
        }

        // =========================================================
        // PUT: api/Users/5
        // Uses UpdateUserDto
        // =========================================================

        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateUser(
            int id,
            [FromBody] UpdateUserDto dto)
        {
            if (!ModelState.IsValid)
            {
                return ValidationProblem(ModelState);
            }

            var user = await _context.Users
                .FirstOrDefaultAsync(u => u.Id == id);

            if (user == null)
            {
                return NotFound(
                    new
                    {
                        message = "User not found."
                    });
            }

            // Check whether another user already uses this email
            var emailExists = await _context.Users
                .AnyAsync(u =>
                    u.Email == dto.Email &&
                    u.Id != id);

            if (emailExists)
            {
                return BadRequest(
                    new
                    {
                        message = "Email already exists."
                    });
            }

            user.FirstName = dto.FirstName.Trim();
            user.LastName = dto.LastName.Trim();
            user.Email = dto.Email.Trim();
            user.Phone = dto.Phone.Trim();
            user.IsActive = dto.IsActive;

            // Password is optional during update.
            // If provided, hash the new password.
            if (!string.IsNullOrWhiteSpace(dto.Password))
            {
                user.Password =
                    _passwordService
                        .HashPassword(dto.Password);
            }

            await _context.SaveChangesAsync();

            return NoContent();
        }

        // =========================================================
        // DELETE: api/Users/5
        // =========================================================

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteUser(int id)
        {
            var user = await _context.Users
                .FirstOrDefaultAsync(u => u.Id == id);

            if (user == null)
            {
                return NotFound(
                    new
                    {
                        message = "User not found."
                    });
            }

            _context.Users.Remove(user);

            await _context.SaveChangesAsync();

            return NoContent();
        }
    }
}