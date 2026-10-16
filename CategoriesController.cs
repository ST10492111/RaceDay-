using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using RaceDay.API.Data;
using RaceDay.API.Models;
using System.Security.Claims;

namespace RaceDay.API.Controllers
{
    [Route("api")]
    [ApiController]
    public class CategoriesController : ControllerBase
    {
        private readonly RaceDayDbContext _context;

        public CategoriesController(RaceDayDbContext context)
        {
            _context = context;
        }

        public class CategoryDto
        {
            public string Name { get; set; } = string.Empty;
            public decimal Distance { get; set; }
            public decimal EntryFee { get; set; }
            public int MaxCapacity { get; set; } = 500;
        }

        // GET: api/events/5/categories
        [HttpGet("events/{eventId}/categories")]
        [AllowAnonymous]
        public async Task<IActionResult> GetCategoriesForEvent(int eventId)
        {
            var eventExists = await _context.Events.AnyAsync(e => e.EventId == eventId);
            if (!eventExists)
                return NotFound(new { message = "Event not found." });

            var categories = await _context.Categories
                .Where(c => c.EventId == eventId)
                .Select(c => new
                {
                    c.CategoryId,
                    c.EventId,
                    c.Name,
                    c.Distance,
                    c.EntryFee,
                    c.MaxCapacity
                })
                .ToListAsync();

            return Ok(categories);
        }

        // POST: api/events/5/categories
        [HttpPost("events/{eventId}/categories")]
        [Authorize(Roles = "Organiser")]
        public async Task<IActionResult> CreateCategory(int eventId, [FromBody] CategoryDto dto)
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (userIdClaim == null) return Unauthorized();

            int organiserId = int.Parse(userIdClaim);

            var ev = await _context.Events.FindAsync(eventId);
            if (ev == null)
                return NotFound(new { message = "Event not found." });

            if (ev.OrganiserId != organiserId)
                return Forbid();

            var category = new Category
            {
                EventId = eventId,
                Name = dto.Name,
                Distance = dto.Distance,
                EntryFee = dto.EntryFee,
                MaxCapacity = dto.MaxCapacity,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };

            _context.Categories.Add(category);
            await _context.SaveChangesAsync();

            return CreatedAtAction(nameof(GetCategoriesForEvent), new { eventId = eventId }, new
            {
                message = "Category created successfully.",
                categoryId = category.CategoryId
            });
        }

        // PUT: api/categories/5
        [HttpPut("categories/{id}")]
        [Authorize(Roles = "Organiser")]
        public async Task<IActionResult> UpdateCategory(int id, [FromBody] CategoryDto dto)
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (userIdClaim == null) return Unauthorized();

            int organiserId = int.Parse(userIdClaim);

            var category = await _context.Categories
                .Include(c => c.Event)
                .FirstOrDefaultAsync(c => c.CategoryId == id);

            if (category == null)
                return NotFound(new { message = "Category not found." });

            if (category.Event == null || category.Event.OrganiserId != organiserId)
                return Forbid();

            category.Name = dto.Name;
            category.Distance = dto.Distance;
            category.EntryFee = dto.EntryFee;
            category.MaxCapacity = dto.MaxCapacity;
            category.UpdatedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync();

            return Ok(new { message = "Category updated successfully." });
        }

        // DELETE: api/categories/5
        [HttpDelete("categories/{id}")]
        [Authorize(Roles = "Organiser")]
        public async Task<IActionResult> DeleteCategory(int id)
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (userIdClaim == null) return Unauthorized();

            int organiserId = int.Parse(userIdClaim);

            var category = await _context.Categories
                .Include(c => c.Event)
                .FirstOrDefaultAsync(c => c.CategoryId == id);

            if (category == null)
                return NotFound(new { message = "Category not found." });

            if (category.Event == null || category.Event.OrganiserId != organiserId)
                return Forbid();

            _context.Categories.Remove(category);
            await _context.SaveChangesAsync();

            return NoContent();
        }
    }
}
