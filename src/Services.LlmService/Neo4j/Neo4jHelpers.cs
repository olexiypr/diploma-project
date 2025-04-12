using System.Text;
using System.Text.RegularExpressions;
using Neo4j.Driver;
using Services.LlmService.Services;

namespace Services.LlmService.Neo4j;

public class Neo4jHelpers
{
    public static string CreateName(string text)
    {
        if (string.IsNullOrEmpty(text))
            return text;

        var words = text.Split(new[] { ' ', '-', '_' }, StringSplitOptions.RemoveEmptyEntries);

        var nameText = new StringBuilder();
        
        foreach (var word in words)
        {
            var lword = word;
            if (char.IsDigit(word[0]))
            {
                lword = "_" + word;
            }

            nameText.Append(lword.ToLower());
        }
        return Regex.Replace(nameText.ToString(), "[^a-zA-Z0-9_]", "");
    }

    public static async Task CreateVectorIndex(IDriver driver, ICypherQueryGenerator cypherQueryGenerator)
    {
        var createVectorIndex = cypherQueryGenerator.GenerateCypherQueryToCreateVectorIndex();
        await driver.ExecutableQuery(createVectorIndex).ExecuteAsync();
    }
}