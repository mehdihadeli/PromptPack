
using PromptPack.Models;

namespace PromptPack.Services;

public class TokenEstimator
{
    public int EstimateTokens(string content)
    {
        // More sophisticated token estimation
        var words = content.Split(new[] { ' ', '\n', '\r', '\t' }, StringSplitOptions.RemoveEmptyEntries);
        var codeTokens = System.Text.RegularExpressions.Regex.Matches(content, @"\b\w+\b|[^\w\s]");
        
        // Average: code has more tokens per word than natural language
        return (int)(words.Length * 1.3 + codeTokens.Count * 0.7);
    }
    
    public Dictionary<string, int> EstimatePerFile(RepositoryContext context)
    {
        return context.Files.ToDictionary(
            f => f.RelativePath,
            f => EstimateTokens(f.Content)
        );
    }
}