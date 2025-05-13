using EventBus;

namespace Services.MessagesService.EventBus.Events;

public class TextGenerationCompletedIntegrationEvent : IntegrationEvent
{
    public string Text { get; set; }
    public string TopicId { get; set; }
}