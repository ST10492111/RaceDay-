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
    public class ResultsController : ControllerBase
    {
        private readonly RaceDayDbContext _context;

        public ResultsController(RaceDayDbContext context)
        {
            _context = context;
        }

        public class ResultDto
        {
            public TimeSpan? FinishTime { get; set; }
            public int? Position { get; set; }
            public string Status { get; set; } = "Finished";
            public string? Notes { get; set; }
        }

        // GET: api/events/5/results (public - everyone can see results)
        [HttpGet("events/{eventId}/results")]
        [AllowAnonymous]
        public async Task<IActionResult> GetEventResults(int eventId)
        {
            var eventExists = await _context.Events.AnyAsync(e => e.EventId == eventId);
            if (!eventExists)
                return NotFound(new { message = "Event not found." });

            var results = await _context.Results
                .Include(r => r.Enrolment)
                    .ThenInclude(e => e!.Participant)
                .Include(r => r.Enrolment)
                    .ThenInclude(e => e!.Category)
                .Where(r => r.Enrolment != null && r.Enrolment.EventId == eventId)
                .OrderBy(r => r.Position ?? int.MaxValue)
                .Select(r => new
                {
                    r.ResultId,
                    r.FinishTime,
                    r.Position,
                    r.Status,
                    r.Notes,
                    Participant = r.Enrolment == null || r.Enrolment.Participant == null ? null : new
                    {
                        r.Enrolment.Participant.UserId,
                        r.Enrolment.Participant.FirstName,
                        r.Enrolment.Participant.LastName
                    },
                    Category = r.Enrolment == null || r.Enrolment.Category == null ? null : new
                    {
                        r.Enrolment.Category.CategoryId,
                        r.Enrolment.Category.Name,
                        r.Enrolment.Category.Distance
                    }
                })
                .ToListAsync();

            return Ok(results);
        }

        // GET: api/users/results (current user's results)
        [HttpGet("users/results")]
        [Authorize]
        public async Task<IActionResult> GetMyResults()
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (userIdClaim == null) return Unauthorized();

            int userId = int.Parse(userIdClaim);

            var results = await _context.Results
                .Include(r => r.Enrolment)
                    .ThenInclude(e => e!.Event)
                .Include(r => r.Enrolment)
                    .ThenInclude(e => e!.Category)
                .Where(r => r.Enrolment != null && r.Enrolment.ParticipantId == userId)
                .Select(r => new
                {
                    r.ResultId,
                    r.FinishTime,
                    r.Position,
                    r.Status,
                    r.Notes,
                    Event = r.Enrolment == null || r.Enrolment.Event == null ? null : new
                    {
                        r.Enrolment.Event.EventId,
                        r.Enrolment.Event.Title,
                        r.Enrolment.Event.EventDate,
                        r.Enrolment.Event.Location
                    },
                    Category = r.Enrolment == null || r.Enrolment.Category == null ? null : new
                    {
                        r.Enrolment.Category.CategoryId,
                        r.Enrolment.Category.Name,
                        r.Enrolment.Category.Distance
                    }
                })
                .ToListAsync();

            return Ok(results);
        }

        // POST: api/enrolments/5/results (Organiser records a result)
        [HttpPost("enrolments/{enrolmentId}/results")]
        [Authorize(Roles = "Organiser")]
        public async Task<IActionResult> CreateResult(int enrolmentId, [FromBody] ResultDto dto)
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (userIdClaim == null) return Unauthorized();

            int organiserId = int.Parse(userIdClaim);

            var enrolment = await _context.Enrolments
                .Include(e => e.Event)
                .FirstOrDefaultAsync(e => e.EnrolmentId == enrolmentId);

            if (enrolment == null)
                return NotFound(new { message = "Enrolment not found." });

            if (enrolment.Event == null || enrolment.Event.OrganiserId != organiserId)
                return Forbid();

            // Check if a result already exists for this enrolment
            var existingResult = await _context.Results
                .FirstOrDefaultAsync(r => r.EnrolmentId == enrolmentId);

            if (existingResult != null)
                return Conflict(new { message = "A result already exists for this enrolment." });

            var validStatuses = new[] { "DNS", "DNF", "Finished", "Disqualified" };
            if (!validStatuses.Contains(dto.Status))
                return BadRequest(new { message = "Invalid status. Must be DNS, DNF, Finished or Disqualified." });

            var result = new Result
            {
                EnrolmentId = enrolmentId,
                FinishTime = dto.FinishTime,
                Position = dto.Position,
                Status = dto.Status,
                Notes = dto.Notes,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };

            _context.Results.Add(result);
            await _context.SaveChangesAsync();

            return CreatedAtAction(nameof(GetEventResults), new { eventId = enrolment.EventId }, new
            {
                message = "Result recorded successfully.",
                resultId = result.ResultId
            });
        }

        // PUT: api/results/5 (Organiser updates a result)
        [HttpPut("results/{id}")]
        [Authorize(Roles = "Organiser")]
        public async Task<IActionResult> UpdateResult(int id, [FromBody] ResultDto dto)
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (userIdClaim == null) return Unauthorized();

            int organiserId = int.Parse(userIdClaim);

            var result = await _context.Results
                .Include(r => r.Enrolment)
                    .ThenInclude(e => e!.Event)
                .FirstOrDefaultAsync(r => r.ResultId == id);

            if (result == null)
                return NotFound(new { message = "Result not found." });

            if (result.Enrolment?.Event == null || result.Enrolment.Event.OrganiserId != organiserId)
                return Forbid();

            var validStatuses = new[] { "DNS", "DNF", "Finished", "Disqualified" };
            if (!validStatuses.Contains(dto.Status))
                return BadRequest(new { message = "Invalid status." });

            result.FinishTime = dto.FinishTime;
            result.Position = dto.Position;
            result.Status = dto.Status;
            result.Notes = dto.Notes;
            result.UpdatedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync();

            return Ok(new { message = "Result updated successfully." });
        }
    }
}