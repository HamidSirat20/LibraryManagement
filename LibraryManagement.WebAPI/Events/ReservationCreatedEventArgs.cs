namespace LibraryManagement.WebAPI.Events;
public class ReservationCreatedEventArgs : IEvent
{
    public DateTime OccurredAt { get; set; } = DateTime.UtcNow;
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public string BookTitle { get; set; } = string.Empty;
    public DateTime ReservedAt { get; set; }
    public string UserEmail { get; set; } = string.Empty;
    public int QueuePosition { get; set; }
}

