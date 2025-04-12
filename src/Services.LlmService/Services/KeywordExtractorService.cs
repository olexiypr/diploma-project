using System.Text.Json;
using Microsoft.SemanticKernel.ChatCompletion;

namespace Services.LlmService.Services;

public class KeywordExtractorService(IChatCompletionService completionService) : IKeywordExtractorService
{
    private record Keyword(string word);

    private readonly int _retryCount = 2;
    public async Task<IEnumerable<string>> ExtractKeywords(string text)
    {
        var keywords = await TryExtractKeywordsUsingLlmWithRetryIfCannot(text);
        return keywords;
    }
    
    private async Task<IEnumerable<string>> TryExtractKeywordsUsingLlmWithRetryIfCannot(string text)
    {
        var exceptions = new List<Exception>();
        var prompt = $@"
                        Given a user's story, pick or use {text.Split(" ").Length / 2} word to create a keywords list to capture what the user's story about.
                        Those words should be names, events, items, weather conditions, roles
                        STORY: {text}
                        ######################
                        GUIDELINES:
                        - Use the full form for entities (ie., 'Artificial Intelligence' instead of 'AI')
                        - Use full names and roles (ie., 'Jason Haley chief engineer' instead of 'Jason' or 'chief'
                        - Keep entities and relation names concise (1-5 words max)
                        - Keep phrases concise and extract from them keywords into separate words
                        - Output in JSON format: [{{""word"": """"}}]
                        ######################
                        EXAMPLE:
                        Text: Jason Haley, chief engineer of Jason Haley Consulting, wrote a new blog post titled 'Study Notes: GraphRAG - Property Graphs' about creating a property graph RAG system using Semantic Kernel. 
                        Output:
                        [{{""word"": ""Jason Haley""}},
                         {{""word"": ""Jason Haley chief engineer""}},   
                         {{""word"": ""Chief""}},
                         {{""word"": ""Jason Haley Consulting""}},
                         {{""word"": ""Study Notes: GraphRAG - Property Graphs""}},
                         {{""word"": ""Graph RAG""}},
                         {{""word"": ""Semantic Kernel""}},
                         {{""word"": ""Blog post""}},
                         {{""word"": ""Wrote""}}]
                        ######################
                        KEYWORDS:
                        ";
        for (var attempted = 0; attempted < _retryCount; attempted++)
        {
            try
            {
                var result = await completionService.GetChatMessageContentsAsync(prompt);
                var content = result[0].ToString().Replace("```json", "").Replace("```", "").Replace("'", "").Trim();
                var rows = JsonSerializer.Deserialize<List<Keyword>>(content);
                return rows.Select(r => r.word);
            }
            catch (Exception ex)
            {
                exceptions.Add(ex);
            }
        }
        throw new AggregateException(exceptions);
    }
}