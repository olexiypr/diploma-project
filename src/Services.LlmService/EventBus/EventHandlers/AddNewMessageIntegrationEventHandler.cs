using EventBus;
using Microsoft.SemanticKernel.Embeddings;
using Services.LlmService.EventBus.Events;
#pragma warning disable SKEXP0001

namespace Services.LlmService.EventBus.EventHandlers;

public class AddNewMessageIntegrationEventHandler(ITextEmbeddingGenerationService embeddingGenerationService) : IIntegrationEventHandler<AddNewMessageIntegrationEvent>
{
    public Task Handle(AddNewMessageIntegrationEvent integrationEvent)
    {
        throw new NotImplementedException();
    }
}