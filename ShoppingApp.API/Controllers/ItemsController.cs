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
            var activeItems = await _context.Items
                .Where(item => !item.Deleted)
                .ToListAsync();
            return Ok(activeItems);
        }

        // POST: api/items
        [HttpPost]
        public async Task<ActionResult<Item>> AddItem([FromBody] Item newItem)
        {
            if (newItem == null)
            {
                return BadRequest("Item data is required.");
            }

            // Add the new item to the database
            _context.Items.Add(newItem);
            await _context.SaveChangesAsync();

            return CreatedAtAction(nameof(GetItems), new { id = newItem.Id }, newItem);
        }

        // DELETE: api/items/{id}
        [HttpDelete("{id}")]
        public async Task<ActionResult> DeleteItem(int id)
        {
            // Try to find the item by its ID
            var item = await _context.Items.FindAsync(id);

            // If the item is not found, return a 404 Not Found response
            if (item == null)
            {
                return NotFound($"Item with ID {id} not found.");
            }

            // Mark the item as deleted (soft delete)
            item.Deleted = true;

            // Save changes to the database
            try
            {
                await _context.SaveChangesAsync();
                // Log to check if the deletion was successful
                Console.WriteLine($"Item with ID {id} marked as deleted.");
            }
            catch (Exception ex)
            {
                // Log the exception in case of an error during saving
                Console.WriteLine($"Error saving item with ID {id}: {ex.Message}");
                return StatusCode(500, "An error occurred while deleting the item.");
            }

            // Return a 204 No Content response to indicate successful deletion
            return NoContent();
        }


        // GET: api/items/calculatePrice
        [HttpGet("calculatePrice")]
        public async Task<ActionResult<decimal>> CalculatePrice()
        {
            var totalPrice = await _context.Items
                .Where(item => !item.Deleted)
                .SumAsync(item => item.Price);

            return Ok(totalPrice);
        }
    }
}
