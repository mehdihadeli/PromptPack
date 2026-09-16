using System.Text;
using Microsoft.Extensions.Logging;
using PromptPack.Models;
using PromptPack.Services.Interfaces;

namespace PromptPack.Services;

public class ContentProcessor(ILogger<ContentProcessor> logger) : IContentProcessor
{
    public async Task<ProcessedFile> ProcessFileAsync(
        string filePath,
        string baseDirectory,
        bool removeComments,
        bool removeEmptyLines,
        bool compress
    )
    {
        logger.LogDebug("Processing file: {FilePath}", filePath);

        var content = await File.ReadAllTextAsync(filePath);

        if (removeComments)
        {
            content = RemoveComments(content, Path.GetExtension(filePath));
        }

        if (removeEmptyLines)
        {
            content = string.Join(
                Environment.NewLine,
                content.Split('\n').Where(line => !string.IsNullOrWhiteSpace(line))
            );
        }

        if (compress)
        {
            content = CompressContent(content, Path.GetExtension(filePath));
        }

        return ProcessedFile.Create(filePath, baseDirectory, content);
    }

    private string RemoveComments(string content, string extension)
    {
        var lines = content.Split('\n');
        var result = new StringBuilder();
        var inMultiLineComment = false;

        foreach (var line in lines)
        {
            var trimmed = line.TrimStart();

            if (inMultiLineComment)
            {
                if (trimmed.Contains("*/"))
                {
                    inMultiLineComment = false;
                    var remaining = trimmed[
                        (trimmed.IndexOf("*/", StringComparison.Ordinal) + 2)..
                    ];
                    if (!string.IsNullOrWhiteSpace(remaining))
                        result.AppendLine(remaining);
                }
                continue;
            }

            if (trimmed.StartsWith("//") || trimmed.StartsWith("#"))
                continue;

            if (trimmed.StartsWith("/*"))
            {
                if (!trimmed.Contains("*/"))
                    inMultiLineComment = true;
                continue;
            }

            if (trimmed.StartsWith("<!--"))
            {
                if (!trimmed.Contains("-->"))
                    inMultiLineComment = true;
                continue;
            }

            result.AppendLine(line);
        }

        return result.ToString();
    }

    private string CompressContent(string content, string extension)
    {
        var lines = content.Split('\n');
        var result = new StringBuilder();
        var skipBlock = false;
        var braceCount = 0;

        foreach (var line in lines)
        {
            var trimmed = line.Trim();

            // Keep important declarations
            if (
                trimmed.Contains("class ")
                || trimmed.Contains("interface ")
                || trimmed.Contains("struct ")
                || trimmed.Contains("enum ")
                || trimmed.Contains("function ")
                || trimmed.Contains("def ")
                || trimmed.Contains("public ")
                || trimmed.Contains("private ")
                || trimmed.Contains("protected ")
                || trimmed.Contains("import ")
                || trimmed.Contains("export ")
            )
            {
                result.AppendLine(line);
                skipBlock = trimmed.Contains('{');
                braceCount = skipBlock ? 1 : 0;
            }
            else if (skipBlock)
            {
                braceCount += trimmed.Count(c => c == '{');
                braceCount -= trimmed.Count(c => c == '}');

                if (braceCount <= 0)
                {
                    skipBlock = false;
                    result.AppendLine("{ ... }");
                }
            }
            else if (trimmed.EndsWith(';'))
            {
                result.AppendLine(line);
            }
        }

        return result.ToString();
    }
}
