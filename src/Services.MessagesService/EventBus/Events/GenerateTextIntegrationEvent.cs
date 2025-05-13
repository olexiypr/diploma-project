using EventBus;

namespace Services.MessagesService.EventBus.Events;

public class GenerateTextIntegrationEvent : IntegrationEvent
{
    public string LastMessageText { get; set; }
    public string TopicId { get; set; }
    public string Description { get; set; }
    public string AdditionalTopicDescription { get; set; }
}