using System.Text;
using PromptPack.Models;
using PromptPack.Services.Interfaces;

namespace PromptPack.Services.OutputGenerators;

public class PlainTextOutputGenerator : IOutputGenerator
{
    public string Style => "plain";

    public string Generate(RepositoryContext context)
    {
        var sb = new StringBuilder();

        sb.AppendLine("================================================================");
        sb.AppendLine("REPOSITORY CONTEXT FOR AI ANALYSIS");
        sb.AppendLine("================================================================");
        sb.AppendLine();
        if (!string.IsNullOrWhiteSpace(context.HeaderText))
        {
            sb.AppendLine(context.HeaderText);
            sb.AppendLine();
        }
        if (!string.IsNullOrWhiteSpace(context.Instructions))
        {
            sb.AppendLine("INSTRUCTIONS");
            sb.AppendLine(context.Instructions);
            sb.AppendLine();
        }
        if (context.IncludeFileSummary)
        {
            sb.AppendLine($"Generated: {context.GeneratedAt:yyyy-MM-dd HH:mm:ss UTC}");
            sb.AppendLine($"Total Files: {context.FileCount}");
            sb.AppendLine($"Total Size: {context.TotalSize:N0} bytes");
            sb.AppendLine();
        }
        if (context.FullDirectoryPaths.Count > 0)
        {
            sb.AppendLine("================================================================");
            sb.AppendLine("FULL DIRECTORY STRUCTURE");
            sb.AppendLine("================================================================");
            sb.AppendLine(context.GetFullDirectoryStructure());
            sb.AppendLine();
        }
        if (context.SecurityFindings.Count > 0)
        {
            sb.AppendLine("SECURITY FINDINGS");
            foreach (var finding in context.SecurityFindings)
                sb.AppendLine($"{finding.RelativePath}: {finding.Rule}");
            sb.AppendLine();
        }
        sb.AppendLine("================================================================");
        if (context.IncludeDirectoryStructure)
        {
            sb.AppendLine("DIRECTORY STRUCTURE");
            sb.AppendLine("================================================================");
            sb.AppendLine(context.GetDirectoryStructure());
            sb.AppendLine();
        }

        if (!context.IncludeFiles)
            return sb.ToString();
        sb.AppendLine("================================================================");
        sb.AppendLine("FILES");
        sb.AppendLine("================================================================");

        foreach (var file in context.Files)
        {
            sb.AppendLine($"================ ================");
            sb.AppendLine($"FILE: {file.RelativePath}");
            sb.AppendLine($"LANGUAGE: {file.Language} | SIZE: {file.Size:N0} bytes");
            sb.AppendLine($"================ ================");
            sb.AppendLine(AddLineNumbers(file.Content, context.ShowLineNumbers));
            sb.AppendLine();
        }

        return sb.ToString();
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
