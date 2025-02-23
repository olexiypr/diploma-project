using EventBus;

namespace Services.LLM.EventBus.Events;

public class GenerateTextIntegrationEvent : IntegrationEvent
{
    public string Text { get; set; }
}