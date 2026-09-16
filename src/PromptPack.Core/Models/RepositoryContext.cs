using System.Text;

namespace PromptPack.Models;

public class RepositoryContext
{
    public string BaseDirectory { get; init; } = string.Empty;
    public IReadOnlyList<ProcessedFile> Files { get; init; } = Array.Empty<ProcessedFile>();
    public DateTime GeneratedAt { get; init; } = DateTime.UtcNow;
    public bool IncludeFileSummary { get; init; } = true;
    public bool IncludeDirectoryStructure { get; init; } = true;
    public bool IncludeFiles { get; init; } = true;
    public bool ShowLineNumbers { get; init; }
    public string? HeaderText { get; init; }
    public string? Instructions { get; init; }
    public IReadOnlyList<string> FullDirectoryPaths { get; init; } = Array.Empty<string>();
    public IReadOnlyList<SecurityFinding> SecurityFindings { get; init; } =
        Array.Empty<SecurityFinding>();

    public long TotalSize => Files.Sum(f => f.Size);
    public int FileCount => Files.Count;

    public string GetDirectoryStructure()
    {
        var directories = new SortedSet<string>();
        var structure = new StringBuilder();

        foreach (var file in Files.OrderBy(f => f.RelativePath))
        {
            var dir = System.IO.Path.GetDirectoryName(file.RelativePath)?.Replace('\\', '/');
            if (!string.IsNullOrEmpty(dir))
            {
                var parts = dir.Split('/');
                var currentPath = string.Empty;
                foreach (var part in parts)
                {
                    currentPath = string.IsNullOrEmpty(currentPath)
                        ? part
                        : $"{currentPath}/{part}";
                    if (directories.Add(currentPath))
                    {
                        var depth = currentPath.Count(c => c == '/');
                        structure.AppendLine($"{new string(' ', depth * 2)}{part}/");
                    }
                }
            }
            structure.AppendLine(
                $"{new string(' ', file.RelativePath.Count(c => c == '/') * 2)}{System.IO.Path.GetFileName(file.RelativePath)}"
            );
        }

        return structure.ToString();
    }

    public string GetFullDirectoryStructure() =>
        string.Join(Environment.NewLine, FullDirectoryPaths.OrderBy(path => path));
}
