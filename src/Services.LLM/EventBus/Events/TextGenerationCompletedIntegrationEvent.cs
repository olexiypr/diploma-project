using EventBus;

namespace Services.LLM.EventBus.Events;

public class TextGenerationCompletedIntegrationEvent : IntegrationEvent
{
    public string Text { get; set; }
}