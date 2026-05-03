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
        sb.AppendLine("> This file is a merged representation of the entire codebase, " +
                      "combining all repository files into a single document for AI analysis.");
        sb.AppendLine();
        
        // File Summary
        sb.AppendLine("## 📊 File Summary");
        sb.AppendLine();
        sb.AppendLine($"- **Generated**: {context.GeneratedAt:yyyy-MM-dd HH:mm:ss UTC}");
        sb.AppendLine($"- **Total Files**: {context.FileCount}");
        sb.AppendLine($"- **Total Size**: {FormatSize(context.TotalSize)}");
        sb.AppendLine();
        
        // Directory Structure
        sb.AppendLine("## 📁 Repository Structure");
        sb.AppendLine();
        sb.AppendLine("```");
        sb.AppendLine(context.GetDirectoryStructure());
        sb.AppendLine("```");
        sb.AppendLine();
        
        // Files
        sb.AppendLine("## 📄 Repository Files");
        sb.AppendLine();
        
        foreach (var file in context.Files)
        {
            sb.AppendLine($"### 📝 {file.RelativePath}");
            sb.AppendLine($"**Language**: {file.Language} | **Size**: {FormatSize(file.Size)}");
            sb.AppendLine();
            sb.AppendLine($"```{file.Language}");
            sb.AppendLine(file.Content);
            sb.AppendLine("```");
            sb.AppendLine();
            sb.AppendLine("---");
            sb.AppendLine();
        }
        
        return sb.ToString();
    }
    
    private static string FormatSize(long bytes) => bytes switch
    {
        < 1024 => $"{bytes} B",
        < 1024 * 1024 => $"{bytes / 1024.0:F1} KB",
        _ => $"{bytes / (1024.0 * 1024):F1} MB"
    };
}