using EventBus;

namespace Services.LlmService.EventBus.Events;

public class AddNewMessageIntegrationEvent : IntegrationEvent
{
    public string TopicId { get; set; }
    public string MessageText { get; set; }
}