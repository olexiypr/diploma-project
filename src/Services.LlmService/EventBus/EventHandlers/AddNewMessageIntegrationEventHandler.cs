using System.Text;
using System.Text.Json;
using EventBus;
using Microsoft.SemanticKernel.ChatCompletion;
using Microsoft.SemanticKernel.Embeddings;
using Neo4j.Driver;
using Services.LlmService.Entities;
using Services.LlmService.EventBus.Events;
using Services.LlmService.Services;

#pragma warning disable SKEXP0001

namespace Services.LlmService.EventBus.EventHandlers;

public class AddNewMessageIntegrationEventHandler(
    IChatCompletionService chatCompletionService,
    ICypherQueryGenerator cypherQueryGenerator,
    IDriver driver,
    ITextSplitter textSplitter) : IIntegrationEventHandler<AddNewMessageIntegrationEvent>
{
    public async Task Handle(AddNewMessageIntegrationEvent integrationEvent)
    {
        Console.WriteLine("AddNewMessageIntegrationEventHandler");
        var chunks = textSplitter.Split(integrationEvent.MessageText, integrationEvent.MessageText.Length / 20, integrationEvent.MessageText.Length / 60);
        foreach (var chunk in chunks)
        {
            var maxTripletsPerChunk = 100;
            //Can add to prompt something like this
            //var preamble = "The given text document contains blog entry summaries with a Title, Author, Posted On date, Topics and Summary. Make sure to add the WRITTEN_BY relationship for the author.";
            var prompt =  $@"Please extract up to {maxTripletsPerChunk} knowledge triplets from the provided text.
                Each triplet should be in the form of (head, relation, tail) with their respective types.
                ######################
                GUIDELINES:
                - Output in JSON format: [{{""head"": """", ""head_type"": """", ""relation"": """", ""tail"": """", ""tail_type"": """"}}]
                - Use the full form for entities (ie., 'Artificial Intelligence' instead of 'AI')
                - Keep entities and relation names concise (3-5 words max)
                - Break down complex phrases into multiple triplets
                - Ensure the knowledge graph is coherent and easily understandable
                ######################
                EXAMPLE:
                Text: Jason Haley, chief engineer of Jason Haley Consulting, wrote a new blog post titled 'Study Notes: GraphRAG - Property Graphs' about creating a property graph RAG system using Semantic Kernel. 
                Output:
                [{{""head"": ""Jason Haley"", ""head_type"": ""PERSON"", ""relation"": ""WORKS_FOR"", ""tail"": ""Jason Haley Consulting"", ""tail_type"": ""COMPANY""}},
                {{""head"": ""Study Notes: GraphRAG - Property Grids"", ""head_type"": ""BLOG_POST"", ""relation"": ""WRITTEN_BY"", ""tail"": ""Jason Haley"", ""tail_type"": ""PERSON""}},
                {{""head"": ""Study Notes: GraphRAG - Property Grids"", ""head_type"": ""BLOG_POST"", ""relation"": ""TOPIC"", ""tail"": ""Semantic Kernel"", ""tail_type"": ""TECHNOLOGY""}},
                {{""head"": ""property grid RAG system"", ""head_type"": ""SOFTWARE_SYSTEM"", ""relation"": ""USES"", ""tail"": ""Semantic Kernel"", ""tail_type"": ""TECHNOLOGY""}}]
                ######################
                Text: {chunk}
                ######################
                Output:";
            var result = await chatCompletionService.GetChatMessageContentsAsync(prompt);
            var content = result[0].ToString().Replace("```json", "").Replace("```", "").Replace("'", "").Trim();
            var rows = JsonSerializer.Deserialize<List<TripletRow>>(content);

            await foreach (var tripletRowQuery in cypherQueryGenerator.GenerateCypherQueryFromTripletRowsForRow(integrationEvent.TopicId, chunk, rows))
            {
                var query = new StringBuilder(); ;
                query.AppendJoin(Environment.NewLine, tripletRowQuery);
                await using var session = driver.AsyncSession();
                await driver.ExecutableQuery(query.ToString()).ExecuteAsync();
            }
        }
    }
}