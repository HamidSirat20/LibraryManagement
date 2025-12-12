namespace LibraryManagement.WebAPI.Events;
public class ReservationReadyEventArgs : IEvent
{
    public DateTime OccurredAt { get; } = DateTime.UtcNow;
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public string BookTitle { get; set; } = string.Empty;
    public DateTime PickUpDeadLine { get; set; }
    public string UserEmail { get; set; } = string.Empty;
}

