using EventBus;

namespace Services.LlmService.EventBus.Events;

public class GenerateTextIntegrationEvent : IntegrationEvent
{
    public string Text { get; set; }
}