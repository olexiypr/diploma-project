namespace Services.LlmService.Services;

public interface IEmbeddingGenerationService
{
    Task<ReadOnlyMemory<float>> GenerateEmbedding(string text);
    Task<IList<ReadOnlyMemory<float>>> GenerateEmbeddings(IList<string> chunks);
}