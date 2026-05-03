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
        sb.AppendLine($"Generated: {context.GeneratedAt:yyyy-MM-dd HH:mm:ss UTC}");
        sb.AppendLine($"Total Files: {context.FileCount}");
        sb.AppendLine($"Total Size: {context.TotalSize:N0} bytes");
        sb.AppendLine();
        sb.AppendLine("================================================================");
        sb.AppendLine("DIRECTORY STRUCTURE");
        sb.AppendLine("================================================================");
        sb.AppendLine(context.GetDirectoryStructure());
        sb.AppendLine();
        sb.AppendLine("================================================================");
        sb.AppendLine("FILES");
        sb.AppendLine("================================================================");
        
        foreach (var file in context.Files)
        {
            sb.AppendLine($"================ ================");
            sb.AppendLine($"FILE: {file.RelativePath}");
            sb.AppendLine($"LANGUAGE: {file.Language} | SIZE: {file.Size:N0} bytes");
            sb.AppendLine($"================ ================");
            sb.AppendLine(file.Content);
            sb.AppendLine();
        }
        
        return sb.ToString();
    }
}