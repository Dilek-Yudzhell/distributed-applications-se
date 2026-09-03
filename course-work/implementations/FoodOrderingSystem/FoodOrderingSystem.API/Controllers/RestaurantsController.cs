using FoodOrderingSystem.API.Data;
using FoodOrderingSystem.API.DTOs;
using FoodOrderingSystem.API.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace FoodOrderingSystem.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class RestaurantsController : ControllerBase
    {
        private readonly ApplicationDbContext _context;

        public RestaurantsController(ApplicationDbContext context)
        {
            _context = context;
        }

        // ==========================================
        // GET ALL RESTAURANTS
        // ==========================================
        [HttpGet]
        public async Task<ActionResult<IEnumerable<RestaurantDto>>> GetRestaurants(
            string? name,
            string? cuisineType,
            int page = 1,
            int pageSize = 5,
            string sortBy = "name")
        {
            if (page < 1)
            {
                return BadRequest("Page must be greater than 0.");
            }

            if (pageSize < 1 || pageSize > 100)
            {
                return BadRequest("PageSize must be between 1 and 100.");
            }

            var query = _context.Restaurants
                .AsNoTracking();

            // Search by restaurant name
            if (!string.IsNullOrWhiteSpace(name))
            {
                query = query.Where(r =>
                    r.Name.Contains(name));
            }

            // Search by cuisine type
            if (!string.IsNullOrWhiteSpace(cuisineType))
            {
                query = query.Where(r =>
                    r.CuisineType.Contains(cuisineType));
            }

            // Sorting
            query = sortBy.ToLower() switch
            {
                "name" =>
                    query.OrderBy(r => r.Name),

                "cuisine" =>
                    query.OrderBy(r => r.CuisineType),

                "rating" =>
                    query.OrderByDescending(r => r.Rating),

                "date" =>
                    query.OrderByDescending(r => r.CreatedDate),

                _ =>
                    query.OrderBy(r => r.Id)
            };

            var restaurants = await query
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .Select(r => new RestaurantDto
                {
                    Id = r.Id,
                    Name = r.Name,
                    Address = r.Address,
                    Phone = r.Phone,
                    CuisineType = r.CuisineType,
                    Rating = r.Rating,
                    CreatedDate = r.CreatedDate,
                    IsActive = r.IsActive
                })
                .ToListAsync();

            return Ok(restaurants);
        }

        // ==========================================
        // GET RESTAURANT BY ID
        // ==========================================
        [HttpGet("{id}")]
        public async Task<ActionResult<RestaurantDto>> GetRestaurant(int id)
        {
            var restaurant = await _context.Restaurants
                .AsNoTracking()
                .Where(r => r.Id == id)
                .Select(r => new RestaurantDto
                {
                    Id = r.Id,
                    Name = r.Name,
                    Address = r.Address,
                    Phone = r.Phone,
                    CuisineType = r.CuisineType,
                    Rating = r.Rating,
                    CreatedDate = r.CreatedDate,
                    IsActive = r.IsActive
                })
                .FirstOrDefaultAsync();

            if (restaurant == null)
            {
                return NotFound();
            }

            return Ok(restaurant);
        }

        // ==========================================
        // CREATE RESTAURANT
        // ==========================================
        [HttpPost]
        public async Task<ActionResult<RestaurantDto>> CreateRestaurant(
            RestaurantDto dto)
        {
            var restaurant = new Restaurant
            {
                Name = dto.Name,
                Address = dto.Address,
                Phone = dto.Phone,
                CuisineType = dto.CuisineType,
                Rating = dto.Rating,
                CreatedDate = DateTime.UtcNow,
                IsActive = dto.IsActive
            };

            _context.Restaurants.Add(restaurant);

            await _context.SaveChangesAsync();

            var result = new RestaurantDto
            {
                Id = restaurant.Id,
                Name = restaurant.Name,
                Address = restaurant.Address,
                Phone = restaurant.Phone,
                CuisineType = restaurant.CuisineType,
                Rating = restaurant.Rating,
                CreatedDate = restaurant.CreatedDate,
                IsActive = restaurant.IsActive
            };

            return CreatedAtAction(
                nameof(GetRestaurant),
                new { id = restaurant.Id },
                result);
        }

        // ==========================================
        // UPDATE RESTAURANT
        // ==========================================
        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateRestaurant(
            int id,
            RestaurantDto dto)
        {
            var restaurant = await _context.Restaurants
                .FindAsync(id);

            if (restaurant == null)
            {
                return NotFound();
            }

            restaurant.Name = dto.Name;
            restaurant.Address = dto.Address;
            restaurant.Phone = dto.Phone;
            restaurant.CuisineType = dto.CuisineType;
            restaurant.Rating = dto.Rating;
            restaurant.IsActive = dto.IsActive;

            await _context.SaveChangesAsync();

            return NoContent();
        }

        // ==========================================
        // DELETE RESTAURANT
        // ==========================================
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteRestaurant(int id)
        {
            var restaurant = await _context.Restaurants
                .FindAsync(id);

            if (restaurant == null)
            {
                return NotFound();
            }

            _context.Restaurants.Remove(restaurant);

            await _context.SaveChangesAsync();

            return NoContent();
        }
    }
}