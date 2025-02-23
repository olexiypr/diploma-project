namespace EventBus;

public interface IIntegrationEventHandler<TEvent> where TEvent : IntegrationEvent
{
    public Task Handle(TEvent integrationEvent);
}