using System.Text.Json;
using PromptPack.Models;
using PromptPack.Services.Interfaces;

namespace PromptPack.Services.OutputGenerators;

public class JsonOutputGenerator : IOutputGenerator
{
    public string Style => "json";
    
    public string Generate(RepositoryContext context)
    {
        var output = new
        {
            fileSummary = new
            {
                purpose = "This file is a merged representation of the entire codebase for AI analysis.",
                generatedAt = context.GeneratedAt.ToString("O"),
                fileCount = context.FileCount,
                totalSize = context.TotalSize
            },
            directoryStructure = context.GetDirectoryStructure(),
            files = context.Files.ToDictionary(
                f => f.RelativePath,
                f => new
                {
                    language = f.Language,
                    size = f.Size,
                    content = f.Content
                }
            )
        };
        
        return JsonSerializer.Serialize(output, new JsonSerializerOptions 
        { 
            WriteIndented = true,
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase
        });
    }
}