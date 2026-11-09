namespace RaceDay.MVC.ViewModels
{
    public class EventViewModel
    {
        public int EventId { get; set; }
        public string Title { get; set; } = string.Empty;
        public string? Description { get; set; }
        public string EventType { get; set; } = string.Empty;
        public decimal Distance { get; set; }
        public DateTime EventDate { get; set; }
        public string Location { get; set; } = string.Empty;
        public string Status { get; set; } = string.Empty;
        public string? OrganiserName { get; set; }
    }
}