using System.Diagnostics.CodeAnalysis;
using EventBus;
using Microsoft.SemanticKernel.ChatCompletion;
using Neo4j.Driver;
using Services.LlmService.EventBus.Events;
using Services.LlmService.Services;

namespace Services.LlmService.EventBus.EventHandlers;

public class GenerateTextIntegrationEventHandler(
    IEventBus eventBus,
    IChatCompletionService completionService,
    IKeywordExtractorService keywordExtractorService,
    ICypherQueryGenerator cypherQueryGenerator,
    IDriver driver)
    : IIntegrationEventHandler<GenerateTextIntegrationEvent>
{
    record ChunkResult(string Triplet, string ChunkText);
    [Experimental("SKEXP0010")]
    public async Task Handle(GenerateTextIntegrationEvent integrationEvent)
    {
        var keywords = await keywordExtractorService.ExtractKeywords(integrationEvent.LastMessageText);
        var graphRelations = await GetRelationsFromDbByKeywords(keywords);
        var chunkResults = graphRelations.ToArray();
        var context = $@"
            ######################
            Structured data:
            {string.Join(Environment.NewLine, chunkResults.Select(c => c.Triplet).Take(50).ToArray())}
            ######################
            Unstructured data:
            {string.Join(Environment.NewLine, chunkResults.Select(c => c.ChunkText).Take(50).ToArray())}
            ";

        var prompt = $@"
            You should ONLY generate story continuation based on input
            To plan the response, begin by examining the Neo4j entity relations and their structured data to understand relations, nodes, and why they related to each other. Follow these steps:

            Analyze the provided Neo4j entity relations and their structured data:

            Look at the nodes, relationships, and properties in the graph.
            Identify the entities and their connections relevant to the story continuation.
            Identify relevant information:

            Extract data points and relationships that are pertinent to the story continuation.
            Consider how these relationships influence the story continuation.
            Synthesize the identified information:
            Combine the extracted information logically.
            Formulate a coherent and comprehensive story continuation.
            ######################
            GUIDELINES:
            - Output in JSON format: {{""story"": """"}}
            - You should ONLY generate story continuation based on input
            - Do not add any clarifications or questions. This message it's all you have to generate story continuation
            ######################
            The story should generally be about the following:
            {integrationEvent.Description}
            ######################
            Follow followig rules in generation you story
            {integrationEvent.AdditionalTopicDescription}
            ######################
            Focus on weaving together the elements in a way that tells a complete and engaging story. Think of
            each node as a potential actor in the world, each relationship as a thread of fate or influence,
            and the overall graph as a hidden story map waiting to be told. Use creativity to animate the data while preserving the logic behind the connections.
            ######################
            You story should be logical continuation to previous story continuation. Use following text as previous story continuation. You story should be same size as previous one, but not smaller than 75 words.
            {integrationEvent.LastMessageText}
            Use the following context as your story world. Base the story entirely on the information below. Do not include any external knowledge.
            {context}
            ######################
            Story:";
        var chatHistory = new ChatHistory();
        chatHistory.AddSystemMessage(prompt);
        chatHistory.AddUserMessage("Generate story continuation based on information that you have");
        var result = await completionService.GetChatMessageContentsAsync(chatHistory);
        Console.WriteLine(result);
    }

    private async Task<IEnumerable<ChunkResult>> GetRelationsFromDbByKeywords(IEnumerable<string> keywords) 
    {
        var uniqueNodes = new HashSet<ChunkResult>();
        foreach (var keyword in keywords)
        {
            var question = await cypherQueryGenerator.GenerateCypherQueryForVectorSearchWord(keyword);
            var chunkResult = await driver.ExecutableQuery(question).ExecuteAsync();

            if (!chunkResult.Result.Any())
                continue;
            
            foreach(var record in chunkResult.Result)
            {
                uniqueNodes.Add(ParseChunkResults(record));  
            }
        }

        return uniqueNodes;
    }

    private ChunkResult ParseChunkResults(IRecord record)
    {
        var tripletValue = record.Values.FirstOrDefault(v => v.Key.ToLower() == "triplet").Value?.ToString() ?? string.Empty;
        var chunkTextValue = record.Values.FirstOrDefault(v => v.Key.ToLower() == "text").Value?.ToString() ?? string.Empty;
        return new ChunkResult(tripletValue, chunkTextValue);
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