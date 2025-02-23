using EventBus;
using Services.LLM.EventBus.Events;

namespace Services.LLM.EventBus.EventHandlers;

public class GenerateTextIntegrationEventHandler(IEventBus eventBus)
    : IIntegrationEventHandler<GenerateTextIntegrationEvent>
{
    public async Task Handle(GenerateTextIntegrationEvent integrationEvent)
    {
        await Task.Delay(1000);
        await SendResponseThroughEventBus("Geenratedeeeeeeeeeeeeeeeeeed Text!");
    }

    private async Task SendResponseThroughEventBus(string generatedText)
    {
        var textGenerationCompletedIntegrationEvent = new TextGenerationCompletedIntegrationEvent
        {
            Text = generatedText
        };
        
        await eventBus.Publish(textGenerationCompletedIntegrationEvent);
    }
}