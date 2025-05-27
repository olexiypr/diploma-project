using EventBus;

namespace Services.MessagesService.EventBus.Events;

public class AddNewMessageIntegrationEvent : IntegrationEvent
{
    public string TopicId { get; set; }
    public string MessageText { get; set; }
}