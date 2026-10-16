namespace RaceDay.API.Models
{
    public class EventImage
    {
        public int ImageId { get; set; }
        public int EventId { get; set; }
        public string ImageUrl { get; set; } = string.Empty;
        public bool IsPrimary { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        // Navigation property
        public Event? Event { get; set; }
    }
}