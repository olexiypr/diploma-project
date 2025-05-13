using EventBus;

namespace Services.LlmService.EventBus.Events;

public class TextGenerationCompletedIntegrationEvent : IntegrationEvent
{
    public string Text { get; set; }
    public string TopicId { get; set; }
}