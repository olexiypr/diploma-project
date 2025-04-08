namespace Services.LlmService.Services;

public interface ITextSplitter
{
    IEnumerable<string> Split(string text, int maxTokens, int tokenOverlap = 0, string[]? splitDelimiters = null);
}