namespace Services.LlmService.Services;

public interface IKeywordExtractorService
{
    Task<IEnumerable<string>> ExtractKeywords(string text);
}