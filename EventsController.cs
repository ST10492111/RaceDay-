using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using RaceDay.API.Data;
using RaceDay.API.Models;
using System.Security.Claims;

namespace RaceDay.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class EventsController : ControllerBase
    {
        private readonly RaceDayDbContext _context;

        public EventsController(RaceDayDbContext context)
        {
            _context = context;
        }

        // DTOs
        public class EventCreateDto
        {
            public string Title { get; set; } = string.Empty;
            public string? Description { get; set; }
            public string EventType { get; set; } = string.Empty;
            public decimal Distance { get; set; }
            public DateTime EventDate { get; set; }
            public string Location { get; set; } = string.Empty;
            public int MaxEntries { get; set; } = 1000;
        }

        public class EventUpdateDto
        {
            public string Title { get; set; } = string.Empty;
            public string? Description { get; set; }
            public string EventType { get; set; } = string.Empty;
            public decimal Distance { get; set; }
            public DateTime EventDate { get; set; }
            public string Location { get; set; } = string.Empty;
            public int MaxEntries { get; set; }
            public string Status { get; set; } = "Upcoming";
        }

        // GET: api/events
        [HttpGet]
        [AllowAnonymous]
        public async Task<IActionResult> GetAllEvents([FromQuery] string? status)
        {
            var query = _context.Events
                .Include(e => e.Organiser)
                .Include(e => e.Categories)
                .AsQueryable();

            if (!string.IsNullOrWhiteSpace(status))
                query = query.Where(e => e.Status == status);

            var events = await query
                .OrderBy(e => e.EventDate)
                .Select(e => new
                {
                    e.EventId,
                    e.Title,
                    e.Description,
                    e.EventType,
                    e.Distance,
                    e.EventDate,
                    e.Location,
                    e.MaxEntries,
                    e.Status,
                    OrganiserName = e.Organiser != null ? e.Organiser.FirstName + " " + e.Organiser.LastName : null,
                    CategoryCount = e.Categories.Count
                })
                .ToListAsync();

            return Ok(events);
        }

        // GET: api/events/5
        [HttpGet("{id}")]
        [AllowAnonymous]
        public async Task<IActionResult> GetEventById(int id)
        {
            var ev = await _context.Events
                .Include(e => e.Organiser)
                .Include(e => e.Categories)
                .Include(e => e.Images)
                .FirstOrDefaultAsync(e => e.EventId == id);

            if (ev == null)
                return NotFound(new { message = "Event not found." });

            return Ok(new
            {
                ev.EventId,
                ev.Title,
                ev.Description,
                ev.EventType,
                ev.Distance,
                ev.EventDate,
                ev.Location,
                ev.MaxEntries,
                ev.Status,
                Organiser = ev.Organiser == null ? null : new
                {
                    ev.Organiser.UserId,
                    ev.Organiser.FirstName,
                    ev.Organiser.LastName,
                    ev.Organiser.Email
                },
                Categories = ev.Categories.Select(c => new
                {
                    c.CategoryId,
                    c.Name,
                    c.Distance,
                    c.EntryFee,
                    c.MaxCapacity
                }),
                Images = ev.Images.Select(i => new
                {
                    i.ImageId,
                    i.ImageUrl,
                    i.IsPrimary
                })
            });
        }

        // POST: api/events
        [HttpPost]
        [Authorize(Roles = "Organiser")]
        public async Task<IActionResult> CreateEvent([FromBody] EventCreateDto dto)
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (userIdClaim == null)
                return Unauthorized(new { message = "Invalid token." });

            int organiserId = int.Parse(userIdClaim);

            var ev = new Event
            {
                OrganiserId = organiserId,
                Title = dto.Title,
                Description = dto.Description,
                EventType = dto.EventType,
                Distance = dto.Distance,
                EventDate = dto.EventDate,
                Location = dto.Location,
                MaxEntries = dto.MaxEntries,
                Status = "Upcoming",
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };

            _context.Events.Add(ev);
            await _context.SaveChangesAsync();

            return CreatedAtAction(nameof(GetEventById), new { id = ev.EventId }, new
            {
                message = "Event created successfully.",
                eventId = ev.EventId
            });
        }

        // PUT: api/events/5
        [HttpPut("{id}")]
        [Authorize(Roles = "Organiser")]
        public async Task<IActionResult> UpdateEvent(int id, [FromBody] EventUpdateDto dto)
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (userIdClaim == null)
                return Unauthorized();

            int organiserId = int.Parse(userIdClaim);

            var ev = await _context.Events.FindAsync(id);
            if (ev == null)
                return NotFound(new { message = "Event not found." });

            if (ev.OrganiserId != organiserId)
                return Forbid();

            ev.Title = dto.Title;
            ev.Description = dto.Description;
            ev.EventType = dto.EventType;
            ev.Distance = dto.Distance;
            ev.EventDate = dto.EventDate;
            ev.Location = dto.Location;
            ev.MaxEntries = dto.MaxEntries;
            ev.Status = dto.Status;
            ev.UpdatedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync();

            return Ok(new { message = "Event updated successfully." });
        }

        // DELETE: api/events/5
        [HttpDelete("{id}")]
        [Authorize(Roles = "Organiser")]
        public async Task<IActionResult> DeleteEvent(int id)
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (userIdClaim == null)
                return Unauthorized();

            int organiserId = int.Parse(userIdClaim);

            var ev = await _context.Events.FindAsync(id);
            if (ev == null)
                return NotFound(new { message = "Event not found." });

            if (ev.OrganiserId != organiserId)
                return Forbid();

            _context.Events.Remove(ev);
            await _context.SaveChangesAsync();

            return NoContent();
        }
    }
}