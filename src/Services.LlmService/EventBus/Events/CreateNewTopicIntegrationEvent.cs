using EventBus;

namespace Services.LlmService.EventBus.Events;

public class CreateNewTopicIntegrationEvent : IntegrationEvent
{
    public string TopicName { get; set; }
    public string TopicId { get; set; }
    public string Description { get; set; }
    public string AdditionalDescription { get; set; }
}