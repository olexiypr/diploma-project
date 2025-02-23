using EventBus;

namespace Services.MessagesService.EventBus.Events;

public class GenerateTextIntegrationEvent : IntegrationEvent
{
    public string Text { get; set; } = "Hello World!";
}