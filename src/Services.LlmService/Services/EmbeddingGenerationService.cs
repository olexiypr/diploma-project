using System.Diagnostics.CodeAnalysis;
using Microsoft.SemanticKernel.Embeddings;

namespace Services.LlmService.Services;

[Experimental("SKEXP0001")]
public class EmbeddingGenerationService(ITextEmbeddingGenerationService embeddingGenerationService) : IEmbeddingGenerationService
{
    public async Task<ReadOnlyMemory<float>> GenerateEmbedding(string text)
    {
        return await embeddingGenerationService.GenerateEmbeddingAsync(text);
    }
    
    public async Task<IList<ReadOnlyMemory<float>>> GenerateEmbeddings(IList<string> chunks)
    {
        //Will I have result in the same order as chunks?
        var result = await embeddingGenerationService.GenerateEmbeddingsAsync(chunks);
        return result;
    }
}