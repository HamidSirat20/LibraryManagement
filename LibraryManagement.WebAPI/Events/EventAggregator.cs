namespace LibraryManagement.WebAPI.Events;
public class EventAggregator : IEventAggregator
{
    private readonly IServiceProvider _serviceProvider;
    private readonly ILogger<EventAggregator> _logger;

    public EventAggregator(IServiceProvider serviceProvider, ILogger<EventAggregator> logger)
    {
        _serviceProvider = serviceProvider;
        _logger = logger;
    }

    public async Task PublishAsync<TEvent>(TEvent evnt) where TEvent : IEvent
    {
        try
        {
            var handlers = _serviceProvider.GetServices<IEventHandler<TEvent>>();
            
            var tasks = handlers.Select(handler =>
                ExecuteHandlerAsync(handler, evnt));

            await Task.WhenAll(tasks);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error publishing event {EventType}", typeof(TEvent).Name);
        }
    }

    private async Task ExecuteHandlerAsync<TEvent>(
        IEventHandler<TEvent> handler,
        TEvent evnt) where TEvent : IEvent
    {
        try
        {
            await handler.HandleAsync(evnt);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error in handler {HandlerType} for event {EventType}",
                handler.GetType().Name, typeof(TEvent).Name);
        }
    }
}
