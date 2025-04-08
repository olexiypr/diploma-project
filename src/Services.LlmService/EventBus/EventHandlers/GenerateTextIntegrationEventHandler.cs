using System.Diagnostics.CodeAnalysis;
using EventBus;
using Microsoft.SemanticKernel;
using Services.LlmService.EventBus.Events;

namespace Services.LlmService.EventBus.EventHandlers;

public class GenerateTextIntegrationEventHandler(IEventBus eventBus)
    : IIntegrationEventHandler<GenerateTextIntegrationEvent>
{
    [Experimental("SKEXP0010")]
    public async Task Handle(GenerateTextIntegrationEvent integrationEvent)
    {
        /*var ollamaConfig = new OllamaConfig
        {
            Endpoint = "http://localhost:11434",
            TextModel = new OllamaModelConfig("deepseek-r1:1.5b"),
            EmbeddingModel = new OllamaModelConfig("nomic-embed-text"),
        };

        var builder = Kernel.CreateBuilder();
        builder.AddOpenAIChatCompletion(modelId: "deepseek-r1", apiKey: null,
            endpoint: new Uri("http://localhost:11434")).Build();

        var memory = new KernelMemoryBuilder()
            .WithOllamaTextGeneration(ollamaConfig)
            .WithOllamaTextEmbeddingGeneration(ollamaConfig)
            .Build();

        var embeddingConfig = new OllamaConfig
        {
            Endpoint = "http://localhost:11434",
            EmbeddingModel = new OllamaModelConfig("nomic-embed-text"),
        };

        var embeddingGenerator = new OllamaTextEmbeddingGenerator(embeddingConfig);


        await Task.Delay(1000);
        await SendResponseThroughEventBus("Geenratedeeeeeeeeeeeeeeeeeed Text!");*/
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