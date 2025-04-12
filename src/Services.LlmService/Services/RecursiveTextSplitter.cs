using System.Text;

namespace Services.LlmService.Services;

public class RecursiveTextSplitter : ITextSplitter
{
    public IEnumerable<string> Split(string text, int maxTokens, int tokenOverlap = 0, string[]? splitDelimiters = null)
    {
        var splitResult = SplitRecursive(text, maxTokens, tokenOverlap, splitDelimiters);
        var resultChunks = splitResult.Distinct().ToList();
        return resultChunks;
    }
    private List<string> SplitRecursive(string text, int maxTokens, int tokenOverlap = 0, string[]? splitDelimiters = null)
    {
        if (GetTextSize(text) <= maxTokens)
        {
            return [text];
        }
        
        splitDelimiters ??= new[] { "\n\n", "\n", ".", " " };
        
        var chunks = new List<string>();
        var current = new StringBuilder();

        foreach (var delimiter in splitDelimiters)
        {
            var parts = text.Split(delimiter, StringSplitOptions.RemoveEmptyEntries);

            foreach (var part in parts)
            {
                var candidate = current.Length == 0 ? part : current + delimiter + part;

                if (GetTextSize(candidate) > maxTokens)
                {
                    if (current.Length > 0)
                    {
                        chunks.Add(current.ToString());
                        current.Clear();
                    }

                    if (GetTextSize(part) > maxTokens)
                    {
                        chunks.AddRange(SplitRecursive(part, maxTokens, tokenOverlap, splitDelimiters.Skip(1).ToArray()));
                    }
                    else
                    {
                        current.Append(part);
                    }
                }
                else
                {
                    current.Clear();
                    current.Append(candidate);
                }
            }

            if (current.Length > 0)
            {
                chunks.Add(current.ToString());   
            }
        }

        //var resultChunks = chunks.Distinct().ToList();
        return chunks;
    }
    
    private IEnumerable<string> ApplyTokenOverlap(IEnumerable<string> chunks, int tokenOverlap)
    {
        var overlappedChunks = new List<string>();
        List<string>? prevTokens = null;

        foreach (var chunk in chunks)
        {
            var tokens = chunk.Split(' ').ToList();

            if (prevTokens != null)
            {
                var overlap = prevTokens.TakeLast(tokenOverlap).ToList();
                tokens.InsertRange(0, overlap);
            }

            overlappedChunks.Add(string.Join(" ", tokens));
            prevTokens = tokens;
        }

        return overlappedChunks;
    }

    private static int GetTextSize(string text)
    {
        return text.Split(" ").Length;
    }
}