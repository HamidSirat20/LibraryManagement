namespace LibraryManagement.WebAPI.Events;

public interface IEventHandler<TEvent> where TEvent : IEvent
{
    Task HandleAsync(TEvent evnt);
}
