namespace LibraryManagement.WebAPI.Events;

public interface IEvent
{
    DateTime OccurredAt { get; }
}
