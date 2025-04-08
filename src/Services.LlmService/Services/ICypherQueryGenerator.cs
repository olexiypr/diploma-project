using Services.LlmService.Entities;

namespace Services.LlmService.Services;

public interface ICypherQueryGenerator
{
    IEnumerable<string> GenerateCypherQueryFromTripletRowsForTopicMessage(string topicId, IEnumerable<TripletRow> tripletRows);
}