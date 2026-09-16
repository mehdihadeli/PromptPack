using System.Text;
using PromptPack.Models;
using PromptPack.Services.Interfaces;

namespace PromptPack.Services.OutputGenerators;

public class MarkdownOutputGenerator : IOutputGenerator
{
    public string Style => "markdown";

    public string Generate(RepositoryContext context)
    {
        var sb = new StringBuilder();

        sb.AppendLine("# Repository Context for AI Analysis");
        sb.AppendLine();

        if (!string.IsNullOrWhiteSpace(context.HeaderText))
        {
            sb.AppendLine(context.HeaderText);
            sb.AppendLine();
        }
        if (!string.IsNullOrWhiteSpace(context.Instructions))
        {
            sb.AppendLine("## Instructions");
            sb.AppendLine();
            sb.AppendLine(context.Instructions);
            sb.AppendLine();
        }
        sb.AppendLine(
            "> This file is a merged representation of the entire codebase, "
                + "combining all repository files into a single document for AI analysis."
        );
        sb.AppendLine();

        if (context.IncludeFileSummary)
        {
            sb.AppendLine("## 📊 File Summary");
            sb.AppendLine();
            sb.AppendLine($"- **Generated**: {context.GeneratedAt:yyyy-MM-dd HH:mm:ss UTC}");
            sb.AppendLine($"- **Total Files**: {context.FileCount}");
            sb.AppendLine($"- **Total Size**: {FormatSize(context.TotalSize)}");
            sb.AppendLine();
        }

        // Directory Structure
        if (context.IncludeDirectoryStructure)
        {
            sb.AppendLine("## 📁 Repository Structure");
            sb.AppendLine();
            sb.AppendLine("```");
            sb.AppendLine(context.GetDirectoryStructure());
            sb.AppendLine("```");
            sb.AppendLine();
        }

        if (context.FullDirectoryPaths.Count > 0)
        {
            sb.AppendLine("## Full Directory Structure");
            sb.AppendLine();
            sb.AppendLine("```");
            sb.AppendLine(context.GetFullDirectoryStructure());
            sb.AppendLine("```");
            sb.AppendLine();
        }

        if (context.SecurityFindings.Count > 0)
        {
            sb.AppendLine("## Security Findings");
            foreach (var finding in context.SecurityFindings)
                sb.AppendLine($"- `{finding.RelativePath}`: {finding.Rule}");
            sb.AppendLine();
        }

        // Files
        if (!context.IncludeFiles)
            return sb.ToString();

        sb.AppendLine("## 📄 Repository Files");
        sb.AppendLine();

        foreach (var file in context.Files)
        {
            sb.AppendLine($"### 📝 {file.RelativePath}");
            sb.AppendLine($"**Language**: {file.Language} | **Size**: {FormatSize(file.Size)}");
            sb.AppendLine();
            sb.AppendLine($"```{file.Language}");
            sb.AppendLine(AddLineNumbers(file.Content, context.ShowLineNumbers));
            sb.AppendLine("```");
            sb.AppendLine();
            sb.AppendLine("---");
            sb.AppendLine();
        }

        return sb.ToString();
    }

    private static string FormatSize(long bytes) =>
        bytes switch
        {
            < 1024 => $"{bytes} B",
            < 1024 * 1024 => $"{bytes / 1024.0:F1} KB",
            _ => $"{bytes / (1024.0 * 1024):F1} MB",
        };

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
