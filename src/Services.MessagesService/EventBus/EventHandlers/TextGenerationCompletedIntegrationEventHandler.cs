using EventBus;
using Services.MessagesService.EventBus.Events;

namespace Services.MessagesService.EventBus.EventHandlers;

public class TextGenerationCompletedIntegrationEventHandler : IIntegrationEventHandler<TextGenerationCompletedIntegrationEvent>
{
    public async Task Handle(TextGenerationCompletedIntegrationEvent integrationEvent)
    {
        Console.WriteLine(integrationEvent.Text);
        await Task.CompletedTask;
    }
}