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
    public class OrderItemsController : ControllerBase
    {
        private readonly ApplicationDbContext _context;

        public OrderItemsController(ApplicationDbContext context)
        {
            _context = context;
        }

        // =========================================================
        // GET ALL ORDER ITEMS
        // Search + Pagination + Sorting
        // =========================================================
        [HttpGet]
        public async Task<ActionResult<IEnumerable<OrderItemDto>>> GetOrderItems(
            [FromQuery] int? orderId = null,
            [FromQuery] int? menuItemId = null,
            [FromQuery] int page = 1,
            [FromQuery] int pageSize = 5,
            [FromQuery] string sortBy = "id")
        {
            // -----------------------------------------------------
            // Validation
            // -----------------------------------------------------

            if (page < 1)
            {
                return BadRequest("Page must be greater than 0.");
            }

            if (pageSize < 1 || pageSize > 100)
            {
                return BadRequest(
                    "PageSize must be between 1 and 100.");
            }

            // -----------------------------------------------------
            // Start query
            // -----------------------------------------------------

            IQueryable<OrderItem> query =
                _context.OrderItems.AsNoTracking();

            // -----------------------------------------------------
            // Search by Order ID
            // -----------------------------------------------------

            if (orderId.HasValue)
            {
                query = query.Where(oi =>
                    oi.OrderId == orderId.Value);
            }

            // -----------------------------------------------------
            // Search by Menu Item ID
            // -----------------------------------------------------

            if (menuItemId.HasValue)
            {
                query = query.Where(oi =>
                    oi.MenuItemId == menuItemId.Value);
            }

            // -----------------------------------------------------
            // Sorting
            // -----------------------------------------------------

            sortBy = (sortBy ?? "id").Trim().ToLowerInvariant();

            query = sortBy switch
            {
                "order" =>
                    query.OrderBy(oi => oi.OrderId),

                "orderid" =>
                    query.OrderBy(oi => oi.OrderId),

                "menuitem" =>
                    query.OrderBy(oi => oi.MenuItemId),

                "menuitemid" =>
                    query.OrderBy(oi => oi.MenuItemId),

                "quantity" =>
                    query.OrderByDescending(oi => oi.Quantity),

                "price" =>
                    query.OrderByDescending(oi => oi.UnitPrice),

                "unitprice" =>
                    query.OrderByDescending(oi => oi.UnitPrice),

                "id" =>
                    query.OrderBy(oi => oi.Id),

                _ =>
                    query.OrderBy(oi => oi.Id)
            };

            // -----------------------------------------------------
            // Pagination
            // -----------------------------------------------------

            var orderItems = await query
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .Select(oi => new OrderItemDto
                {
                    Id = oi.Id,
                    OrderId = oi.OrderId,
                    MenuItemId = oi.MenuItemId,
                    Quantity = oi.Quantity,
                    UnitPrice = oi.UnitPrice,
                    Notes = oi.Notes
                })
                .ToListAsync();

            return Ok(orderItems);
        }


        // =========================================================
        // GET ORDER ITEM BY ID
        // =========================================================

        [HttpGet("{id:int}")]
        public async Task<ActionResult<OrderItemDto>> GetOrderItem(
            int id)
        {
            var orderItem = await _context.OrderItems
                .AsNoTracking()
                .Where(oi => oi.Id == id)
                .Select(oi => new OrderItemDto
                {
                    Id = oi.Id,
                    OrderId = oi.OrderId,
                    MenuItemId = oi.MenuItemId,
                    Quantity = oi.Quantity,
                    UnitPrice = oi.UnitPrice,
                    Notes = oi.Notes
                })
                .FirstOrDefaultAsync();

            if (orderItem == null)
            {
                return NotFound(
                    "The specified order item does not exist.");
            }

            return Ok(orderItem);
        }


        // =========================================================
        // CREATE ORDER ITEM
        // =========================================================

        [HttpPost]
        public async Task<ActionResult<OrderItemDto>> CreateOrderItem(
            [FromBody] OrderItemDto dto)
        {
            // -----------------------------------------------------
            // Check Order
            // -----------------------------------------------------

            var orderExists = await _context.Orders
                .AnyAsync(o => o.Id == dto.OrderId);

            if (!orderExists)
            {
                return BadRequest(
                    "The specified order does not exist.");
            }

            // -----------------------------------------------------
            // Check Menu Item
            // -----------------------------------------------------

            var menuItemExists = await _context.MenuItems
                .AnyAsync(m => m.Id == dto.MenuItemId);

            if (!menuItemExists)
            {
                return BadRequest(
                    "The specified menu item does not exist.");
            }

            // -----------------------------------------------------
            // Create entity
            // -----------------------------------------------------

            var orderItem = new OrderItem
            {
                OrderId = dto.OrderId,
                MenuItemId = dto.MenuItemId,
                Quantity = dto.Quantity,
                UnitPrice = dto.UnitPrice,
                Notes = dto.Notes ?? string.Empty
            };

            _context.OrderItems.Add(orderItem);

            await _context.SaveChangesAsync();

            // -----------------------------------------------------
            // Result DTO
            // -----------------------------------------------------

            var result = new OrderItemDto
            {
                Id = orderItem.Id,
                OrderId = orderItem.OrderId,
                MenuItemId = orderItem.MenuItemId,
                Quantity = orderItem.Quantity,
                UnitPrice = orderItem.UnitPrice,
                Notes = orderItem.Notes
            };

            return CreatedAtAction(
                nameof(GetOrderItem),
                new { id = orderItem.Id },
                result);
        }


        // =========================================================
        // UPDATE ORDER ITEM
        // =========================================================

        [HttpPut("{id:int}")]
        public async Task<IActionResult> UpdateOrderItem(
            int id,
            [FromBody] OrderItemDto dto)
        {
            // -----------------------------------------------------
            // Find existing item
            // -----------------------------------------------------

            var orderItem = await _context.OrderItems
                .FirstOrDefaultAsync(oi => oi.Id == id);

            if (orderItem == null)
            {
                return NotFound(
                    "The specified order item does not exist.");
            }

            // -----------------------------------------------------
            // Check Order
            // -----------------------------------------------------

            var orderExists = await _context.Orders
                .AnyAsync(o => o.Id == dto.OrderId);

            if (!orderExists)
            {
                return BadRequest(
                    "The specified order does not exist.");
            }

            // -----------------------------------------------------
            // Check Menu Item
            // -----------------------------------------------------

            var menuItemExists = await _context.MenuItems
                .AnyAsync(m => m.Id == dto.MenuItemId);

            if (!menuItemExists)
            {
                return BadRequest(
                    "The specified menu item does not exist.");
            }

            // -----------------------------------------------------
            // Update
            // -----------------------------------------------------

            orderItem.OrderId = dto.OrderId;
            orderItem.MenuItemId = dto.MenuItemId;
            orderItem.Quantity = dto.Quantity;
            orderItem.UnitPrice = dto.UnitPrice;
            orderItem.Notes = dto.Notes ?? string.Empty;

            await _context.SaveChangesAsync();

            return NoContent();
        }


        // =========================================================
        // DELETE ORDER ITEM
        // =========================================================

        [HttpDelete("{id:int}")]
        public async Task<IActionResult> DeleteOrderItem(int id)
        {
            var orderItem = await _context.OrderItems
                .FirstOrDefaultAsync(oi => oi.Id == id);

            if (orderItem == null)
            {
                return NotFound(
                    "The specified order item does not exist.");
            }

            _context.OrderItems.Remove(orderItem);

            await _context.SaveChangesAsync();

            return NoContent();
        }
    }
}