using Services.LlmService.Entities;

namespace Services.LlmService.Services;

public interface ICypherQueryGenerator
{
    string GenerateCypherQueryToCreateVectorIndex();
    Task<string> GenerateCypherQueryForVectorSearchWord(string word);

    IAsyncEnumerable<IEnumerable<string>> GenerateCypherQueryFromTripletRowsForRow(string topicId, string chunkText,
        IEnumerable<TripletRow> tripletRows);
}