using System.Security.Claims;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ShoppingApp.API.Models;

namespace ShoppingApp.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ItemsController : ControllerBase
    {
        private readonly AppDbContext _context;

        public ItemsController(AppDbContext context)

        {
            _context = context;
        }

        // GET: api/items
        [HttpGet]
        public async Task<ActionResult<IEnumerable<Item>>> GetItems()
        {
            try
            {
                // Get the current user from the token
                var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

                if (userId == null)
                {
                    return Unauthorized("Token is missing or invalid.");
                }

                // Fetch only items belonging to the current authenticated user
                var activeItems = await _context.Items
                    .Where(item => !item.Deleted && item.UserId.ToString() == userId)
                    .ToListAsync();

                return Ok(activeItems);
            }
            catch (Exception ex)
            {
                // Log the exception
                Console.WriteLine($"Error retrieving items: {ex.Message}");
                return StatusCode(500, "An error occurred while retrieving items.");
            }
        }

        // POST: api/items
        [HttpPost]
        public async Task<ActionResult<Item>> AddItem([FromBody] Item newItem)
        {
            try
            {
                if (newItem == null)
                {
                    return BadRequest("Item data is required.");
                }

                // Get the current user from the token
                var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

                if (userId == null)
                {
                    return Unauthorized("Token is missing or invalid.");
                }

                newItem.UserId = int.Parse(userId);  // Assign the current user's ID to the item

                // Add the new item to the database
                _context.Items.Add(newItem);
                await _context.SaveChangesAsync();

                return CreatedAtAction(nameof(GetItems), new { id = newItem.Id }, newItem);
            }
            catch (Exception ex)
            {
                // Log the exception
                Console.WriteLine($"Error adding item: {ex.Message}");
                return StatusCode(500, "An error occurred while adding the item.");
            }
        }

        // DELETE: api/items/{id}
        [HttpDelete("{id}")]
        public async Task<ActionResult> DeleteItem(int id)
        {
            try
            {
                // Try to find the item by its ID
                var item = await _context.Items.FindAsync(id);

                // If the item is not found, return a 404 Not Found response
                if (item == null)
                {
                    return NotFound($"Item with ID {id} not found.");
                }

                // Check if the item belongs to the current user
                var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
                if (userId == null)
                {
                    return Unauthorized("Token is missing or invalid.");
                }

                if (item.UserId.ToString() != userId)
                {
                    return Unauthorized("You are not authorized to delete this item.");
                }

                // Mark the item as deleted (soft delete)
                item.Deleted = true;

                // Save changes to the database
                await _context.SaveChangesAsync();

                // Log to check if the deletion was successful
                Console.WriteLine($"Item with ID {id} marked as deleted.");

                // Return a 204 No Content response to indicate successful deletion
                return NoContent();
            }
            catch (Exception ex)
            {
                // Log the exception
                Console.WriteLine($"Error deleting item with ID {id}: {ex.Message}");
                return StatusCode(500, "An error occurred while deleting the item.");
            }
        }

        // GET: api/items/calculatePrice
        [HttpGet("calculatePrice")]
        public async Task<ActionResult<decimal>> CalculatePrice()
        {
            try
            {
                var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

                if (userId == null)
                {
                    return Unauthorized("Token is missing or invalid.");
                }

                // Calculate the total price for the current user's items
                var totalPrice = await _context.Items
                    .Where(item => !item.Deleted && item.UserId.ToString() == userId)
                    .SumAsync(item => item.Price);

                return Ok(totalPrice);
            }
            catch (Exception ex)
            {
                // Log the exception
                Console.WriteLine($"Error calculating price: {ex.Message}");
                return StatusCode(500, "An error occurred while calculating the price.");
            }
        }
    }
}
