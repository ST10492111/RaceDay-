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
    public class EnrolmentsController : ControllerBase
    {
        private readonly RaceDayDbContext _context;

        public EnrolmentsController(RaceDayDbContext context)
        {
            _context = context;
        }

        public class EnrolDto
        {
            public int CategoryId { get; set; }
        }

        public class UpdateStatusDto
        {
            public string Status { get; set; } = string.Empty;
        }

        // GET: api/enrolments (current user's enrolments)
        [HttpGet("enrolments")]
        [Authorize]
        public async Task<IActionResult> GetMyEnrolments()
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (userIdClaim == null) return Unauthorized();

            int userId = int.Parse(userIdClaim);

            var enrolments = await _context.Enrolments
                .Include(e => e.Event)
                .Include(e => e.Category)
                .Include(e => e.Result)
                .Where(e => e.ParticipantId == userId)
                .Select(e => new
                {
                    e.EnrolmentId,
                    e.Status,
                    e.RegisteredAt,
                    Event = e.Event == null ? null : new
                    {
                        e.Event.EventId,
                        e.Event.Title,
                        e.Event.EventDate,
                        e.Event.Location,
                        e.Event.EventType
                    },
                    Category = e.Category == null ? null : new
                    {
                        e.Category.CategoryId,
                        e.Category.Name,
                        e.Category.Distance,
                        e.Category.EntryFee
                    },
                    Result = e.Result == null ? null : new
                    {
                        e.Result.ResultId,
                        e.Result.FinishTime,
                        e.Result.Position,
                        e.Result.Status
                    }
                })
                .ToListAsync();

            return Ok(enrolments);
        }

        // GET: api/events/5/enrolments (all enrolments for an event - Organiser)
        [HttpGet("events/{eventId}/enrolments")]
        [Authorize(Roles = "Organiser")]
        public async Task<IActionResult> GetEventEnrolments(int eventId)
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (userIdClaim == null) return Unauthorized();

            int organiserId = int.Parse(userIdClaim);

            var ev = await _context.Events.FindAsync(eventId);
            if (ev == null)
                return NotFound(new { message = "Event not found." });

            if (ev.OrganiserId != organiserId)
                return Forbid();

            var enrolments = await _context.Enrolments
                .Include(e => e.Participant)
                .Include(e => e.Category)
                .Where(e => e.EventId == eventId)
                .Select(e => new
                {
                    e.EnrolmentId,
                    e.Status,
                    e.RegisteredAt,
                    Participant = e.Participant == null ? null : new
                    {
                        e.Participant.UserId,
                        e.Participant.FirstName,
                        e.Participant.LastName,
                        e.Participant.Email
                    },
                    Category = e.Category == null ? null : new
                    {
                        e.Category.CategoryId,
                        e.Category.Name,
                        e.Category.Distance
                    }
                })
                .ToListAsync();

            return Ok(enrolments);
        }

        // POST: api/events/5/enrol (Participant enrols in an event)
        [HttpPost("events/{eventId}/enrol")]
        [Authorize(Roles = "Participant")]
        public async Task<IActionResult> Enrol(int eventId, [FromBody] EnrolDto dto)
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (userIdClaim == null) return Unauthorized();

            int participantId = int.Parse(userIdClaim);

            var ev = await _context.Events.FindAsync(eventId);
            if (ev == null)
                return NotFound(new { message = "Event not found." });

            var category = await _context.Categories.FindAsync(dto.CategoryId);
            if (category == null || category.EventId != eventId)
                return BadRequest(new { message = "Category not found or does not belong to this event." });

            // Check if already enrolled
            var existing = await _context.Enrolments
                .FirstOrDefaultAsync(e => e.ParticipantId == participantId && e.EventId == eventId);

            if (existing != null)
                return Conflict(new { message = "You are already enrolled in this event." });

            // Check capacity
            var categoryCount = await _context.Enrolments
                .CountAsync(e => e.CategoryId == dto.CategoryId && e.Status != "Cancelled");

            if (categoryCount >= category.MaxCapacity)
                return BadRequest(new { message = "This category is full." });

            var enrolment = new Enrolment
            {
                ParticipantId = participantId,
                EventId = eventId,
                CategoryId = dto.CategoryId,
                Status = "Pending",
                RegisteredAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };

            _context.Enrolments.Add(enrolment);
            await _context.SaveChangesAsync();

            return CreatedAtAction(nameof(GetMyEnrolments), null, new
            {
                message = "Enrolment successful.",
                enrolmentId = enrolment.EnrolmentId
            });
        }

        // PUT: api/enrolments/5/cancel
        [HttpPut("enrolments/{id}/cancel")]
        [Authorize]
        public async Task<IActionResult> CancelEnrolment(int id)
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (userIdClaim == null) return Unauthorized();

            int userId = int.Parse(userIdClaim);

            var enrolment = await _context.Enrolments
                .Include(e => e.Event)
                .FirstOrDefaultAsync(e => e.EnrolmentId == id);

            if (enrolment == null)
                return NotFound(new { message = "Enrolment not found." });

            // Either the participant themselves or the organiser can cancel
            bool isParticipant = enrolment.ParticipantId == userId;
            bool isOrganiser = enrolment.Event != null && enrolment.Event.OrganiserId == userId;

            if (!isParticipant && !isOrganiser)
                return Forbid();

            enrolment.Status = "Cancelled";
            enrolment.UpdatedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync();

            return Ok(new { message = "Enrolment cancelled." });
        }

        // PUT: api/enrolments/5/status (Organiser updates status)
        [HttpPut("enrolments/{id}/status")]
        [Authorize(Roles = "Organiser")]
        public async Task<IActionResult> UpdateStatus(int id, [FromBody] UpdateStatusDto dto)
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (userIdClaim == null) return Unauthorized();

            int organiserId = int.Parse(userIdClaim);

            var enrolment = await _context.Enrolments
                .Include(e => e.Event)
                .FirstOrDefaultAsync(e => e.EnrolmentId == id);

            if (enrolment == null)
                return NotFound(new { message = "Enrolment not found." });

            if (enrolment.Event == null || enrolment.Event.OrganiserId != organiserId)
                return Forbid();

            var validStatuses = new[] { "Pending", "Confirmed", "Cancelled", "Waitlisted" };
            if (!validStatuses.Contains(dto.Status))
                return BadRequest(new { message = "Invalid status." });

            enrolment.Status = dto.Status;
            enrolment.UpdatedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync();

            return Ok(new { message = "Enrolment status updated." });
        }
    }
}