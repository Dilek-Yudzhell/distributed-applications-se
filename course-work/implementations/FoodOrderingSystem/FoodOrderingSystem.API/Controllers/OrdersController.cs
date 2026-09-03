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
    public class OrdersController : ControllerBase
    {
        private readonly ApplicationDbContext _context;

        public OrdersController(ApplicationDbContext context)
        {
            _context = context;
        }

        // ==========================================
        // GET ALL ORDERS
        // ==========================================
        [HttpGet]
        public async Task<ActionResult<IEnumerable<OrderDto>>> GetOrders(
            int? userId,
            string? status,
            int page = 1,
            int pageSize = 5,
            string sortBy = "date")
        {
            if (page < 1)
            {
                return BadRequest("Page must be greater than 0.");
            }

            if (pageSize < 1 || pageSize > 100)
            {
                return BadRequest("PageSize must be between 1 and 100.");
            }

            var query = _context.Orders
                .AsNoTracking();

            // Search by user ID
            if (userId.HasValue)
            {
                query = query.Where(o =>
                    o.UserId == userId.Value);
            }

            // Search by status
            if (!string.IsNullOrWhiteSpace(status))
            {
                query = query.Where(o =>
                    o.Status.Contains(status));
            }

            // Sorting
            query = sortBy.ToLower() switch
            {
                "userid" =>
                    query.OrderBy(o => o.UserId),

                "price" =>
                    query.OrderByDescending(o => o.TotalPrice),

                "status" =>
                    query.OrderBy(o => o.Status),

                "date" =>
                    query.OrderByDescending(o => o.OrderDate),

                _ =>
                    query.OrderByDescending(o => o.Id)
            };

            var orders = await query
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .Select(o => new OrderDto
                {
                    Id = o.Id,
                    UserId = o.UserId,
                    TotalPrice = o.TotalPrice,
                    Status = o.Status,
                    DeliveryAddress = o.DeliveryAddress,
                    OrderDate = o.OrderDate
                })
                .ToListAsync();

            return Ok(orders);
        }

        // ==========================================
        // GET ORDER BY ID
        // ==========================================
        [HttpGet("{id}")]
        public async Task<ActionResult<OrderDto>> GetOrder(int id)
        {
            var order = await _context.Orders
                .AsNoTracking()
                .Where(o => o.Id == id)
                .Select(o => new OrderDto
                {
                    Id = o.Id,
                    UserId = o.UserId,
                    TotalPrice = o.TotalPrice,
                    Status = o.Status,
                    DeliveryAddress = o.DeliveryAddress,
                    OrderDate = o.OrderDate
                })
                .FirstOrDefaultAsync();

            if (order == null)
            {
                return NotFound();
            }

            return Ok(order);
        }

        // ==========================================
        // CREATE ORDER
        // ==========================================
        [HttpPost]
        public async Task<ActionResult<OrderDto>> CreateOrder(
            OrderDto dto)
        {
            var userExists = await _context.Users
                .AnyAsync(u => u.Id == dto.UserId);

            if (!userExists)
            {
                return BadRequest(
                    "The specified user does not exist.");
            }

            var order = new Order
            {
                UserId = dto.UserId,
                TotalPrice = dto.TotalPrice,
                Status = dto.Status,
                DeliveryAddress = dto.DeliveryAddress,
                OrderDate = DateTime.UtcNow
            };

            _context.Orders.Add(order);

            await _context.SaveChangesAsync();

            var result = new OrderDto
            {
                Id = order.Id,
                UserId = order.UserId,
                TotalPrice = order.TotalPrice,
                Status = order.Status,
                DeliveryAddress = order.DeliveryAddress,
                OrderDate = order.OrderDate
            };

            return CreatedAtAction(
                nameof(GetOrder),
                new { id = order.Id },
                result);
        }

        // ==========================================
        // UPDATE ORDER
        // ==========================================
        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateOrder(
            int id,
            OrderDto dto)
        {
            var order = await _context.Orders
                .FindAsync(id);

            if (order == null)
            {
                return NotFound();
            }

            var userExists = await _context.Users
                .AnyAsync(u => u.Id == dto.UserId);

            if (!userExists)
            {
                return BadRequest(
                    "The specified user does not exist.");
            }

            order.UserId = dto.UserId;
            order.TotalPrice = dto.TotalPrice;
            order.Status = dto.Status;
            order.DeliveryAddress = dto.DeliveryAddress;

            await _context.SaveChangesAsync();

            return NoContent();
        }

        // ==========================================
        // DELETE ORDER
        // ==========================================
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteOrder(int id)
        {
            var order = await _context.Orders
                .FindAsync(id);

            if (order == null)
            {
                return NotFound();
            }

            _context.Orders.Remove(order);

            await _context.SaveChangesAsync();

            return NoContent();
        }
    }
}