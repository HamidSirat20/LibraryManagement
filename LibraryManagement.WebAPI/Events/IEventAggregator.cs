namespace LibraryManagement.WebAPI.Events;

public interface IEventAggregator
{
    Task PublishAsync<TEvent>(TEvent evnt) where TEvent : IEvent;
}