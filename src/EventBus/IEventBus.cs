namespace EventBus;

public interface IEventBus
{
    public Task Publish<TEvent>(TEvent @event) where TEvent : IntegrationEvent;

    public Task Subscribe<TEvent, TEventHandler>() where TEvent : IntegrationEvent
        where TEventHandler : IIntegrationEventHandler<TEvent>;
}