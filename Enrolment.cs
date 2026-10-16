namespace RaceDay.API.Models
{
    public class Enrolment
    {
        public int EnrolmentId { get; set; }
        public int ParticipantId { get; set; }
        public int EventId { get; set; }
        public int CategoryId { get; set; }
        public string Status { get; set; } = "Pending";
        public DateTime RegisteredAt { get; set; } = DateTime.UtcNow;
        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

        // Navigation properties
        public AppUser? Participant { get; set; }
        public Event? Event { get; set; }
        public Category? Category { get; set; }
        public Result? Result { get; set; }
    }
}
