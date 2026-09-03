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
    public class MenuItemsController : ControllerBase
    {
        private readonly ApplicationDbContext _context;

        public MenuItemsController(ApplicationDbContext context)
        {
            _context = context;
        }

        // ==========================================
        // GET ALL MENU ITEMS
        // ==========================================
        [HttpGet]
        public async Task<ActionResult<IEnumerable<MenuItemDto>>> GetMenuItems(
            string? name,
            string? category,
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

            var query = _context.MenuItems
                .AsNoTracking();

            // Search by menu item name
            if (!string.IsNullOrWhiteSpace(name))
            {
                query = query.Where(m =>
                    m.Name.Contains(name));
            }

            // Search by category
            if (!string.IsNullOrWhiteSpace(category))
            {
                query = query.Where(m =>
                    m.Category.Contains(category));
            }

            // Sorting
            query = sortBy.ToLower() switch
            {
                "name" =>
                    query.OrderBy(m => m.Name),

                "category" =>
                    query.OrderBy(m => m.Category),

                "price" =>
                    query.OrderBy(m => m.Price),

                "date" =>
                    query.OrderByDescending(m => m.Id),

                _ =>
                    query.OrderBy(m => m.Id)
            };

            var menuItems = await query
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .Select(m => new MenuItemDto
                {
                    Id = m.Id,
                    RestaurantId = m.RestaurantId,
                    Name = m.Name,
                    Description = m.Description,
                    Price = m.Price,
                    Category = m.Category,
                    IsAvailable = m.IsAvailable
                })
                .ToListAsync();

            return Ok(menuItems);
        }

        // ==========================================
        // GET MENU ITEM BY ID
        // ==========================================
        [HttpGet("{id}")]
        public async Task<ActionResult<MenuItemDto>> GetMenuItem(int id)
        {
            var menuItem = await _context.MenuItems
                .AsNoTracking()
                .Where(m => m.Id == id)
                .Select(m => new MenuItemDto
                {
                    Id = m.Id,
                    RestaurantId = m.RestaurantId,
                    Name = m.Name,
                    Description = m.Description,
                    Price = m.Price,
                    Category = m.Category,
                    IsAvailable = m.IsAvailable
                })
                .FirstOrDefaultAsync();

            if (menuItem == null)
            {
                return NotFound();
            }

            return Ok(menuItem);
        }

        // ==========================================
        // CREATE MENU ITEM
        // ==========================================
        [HttpPost]
        public async Task<ActionResult<MenuItemDto>> CreateMenuItem(
            MenuItemDto dto)
        {
            var restaurantExists = await _context.Restaurants
                .AnyAsync(r => r.Id == dto.RestaurantId);

            if (!restaurantExists)
            {
                return BadRequest(
                    "The specified restaurant does not exist.");
            }

            var menuItem = new MenuItem
            {
                RestaurantId = dto.RestaurantId,
                Name = dto.Name,
                Description = dto.Description,
                Price = dto.Price,
                Category = dto.Category,
                IsAvailable = dto.IsAvailable
            };

            _context.MenuItems.Add(menuItem);

            await _context.SaveChangesAsync();

            var result = new MenuItemDto
            {
                Id = menuItem.Id,
                RestaurantId = menuItem.RestaurantId,
                Name = menuItem.Name,
                Description = menuItem.Description,
                Price = menuItem.Price,
                Category = menuItem.Category,
                IsAvailable = menuItem.IsAvailable
            };

            return CreatedAtAction(
                nameof(GetMenuItem),
                new { id = menuItem.Id },
                result);
        }

        // ==========================================
        // UPDATE MENU ITEM
        // ==========================================
        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateMenuItem(
            int id,
            MenuItemDto dto)
        {
            var menuItem = await _context.MenuItems
                .FindAsync(id);

            if (menuItem == null)
            {
                return NotFound();
            }

            var restaurantExists = await _context.Restaurants
                .AnyAsync(r => r.Id == dto.RestaurantId);

            if (!restaurantExists)
            {
                return BadRequest(
                    "The specified restaurant does not exist.");
            }

            menuItem.RestaurantId = dto.RestaurantId;
            menuItem.Name = dto.Name;
            menuItem.Description = dto.Description;
            menuItem.Price = dto.Price;
            menuItem.Category = dto.Category;
            menuItem.IsAvailable = dto.IsAvailable;

            await _context.SaveChangesAsync();

            return NoContent();
        }

        // ==========================================
        // DELETE MENU ITEM
        // ==========================================
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteMenuItem(int id)
        {
            var menuItem = await _context.MenuItems
                .FindAsync(id);

            if (menuItem == null)
            {
                return NotFound();
            }

            _context.MenuItems.Remove(menuItem);

            await _context.SaveChangesAsync();

            return NoContent();
        }
    }
}