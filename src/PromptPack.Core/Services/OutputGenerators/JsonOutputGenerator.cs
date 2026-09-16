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
            header = context.HeaderText,
            instructions = context.Instructions,
            fileSummary = context.IncludeFileSummary
                ? new
                {
                    purpose = "This file is a merged representation of the entire codebase for AI analysis.",
                    generatedAt = context.GeneratedAt.ToString("O"),
                    fileCount = context.FileCount,
                    totalSize = context.TotalSize,
                }
                : null,
            directoryStructure = context.IncludeDirectoryStructure
                ? context.GetDirectoryStructure()
                : null,
            fullDirectoryStructure = context.FullDirectoryPaths.Count > 0
                ? context.GetFullDirectoryStructure()
                : null,
            securityFindings = context.SecurityFindings,
            files = context.IncludeFiles
                ? context.Files.ToDictionary(
                    f => f.RelativePath,
                    f => new
                    {
                        language = f.Language,
                        size = f.Size,
                        content = AddLineNumbers(f.Content, context.ShowLineNumbers),
                    }
                )
                : null,
        };

        return JsonSerializer.Serialize(
            output,
            new JsonSerializerOptions
            {
                WriteIndented = true,
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
            }
        );
    }

    private static string AddLineNumbers(string content, bool enabled) =>
        enabled
            ? string.Join(
                Environment.NewLine,
                content
                    .Split('\n')
                    .Select((line, index) => $"{index + 1, 6} | {line.TrimEnd('\r')}")
            )
            : content;
}
