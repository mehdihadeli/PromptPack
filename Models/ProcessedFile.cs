namespace PromptPack.Models;

public record ProcessedFile
{
public required string Path { get; init; }
public required string RelativePath { get; init; }
public required string Content { get; init; }
public required long Size { get; init; }
public required string Language { get; init; }
    
public static ProcessedFile Create(string fullPath, string baseDirectory, string content)
{
    var relativePath = System.IO.Path.GetRelativePath(baseDirectory, fullPath).Replace('\\', '/');
    var extension = System.IO.Path.GetExtension(fullPath).ToLowerInvariant();
        
    return new ProcessedFile
    {
        Path = fullPath,
        RelativePath = relativePath,
        Content = content,
        Size = content.Length,
        Language = GetLanguageFromExtension(extension)
    };
}
    
private static string GetLanguageFromExtension(string extension) => extension switch
{
    ".cs" => "csharp",
    ".js" => "javascript",
    ".ts" => "typescript",
    ".py" => "python",
    ".java" => "java",
    ".go" => "go",
    ".rs" => "rust",
    ".sql" => "sql",
    ".html" => "html",
    ".css" => "css",
    ".json" => "json",
    ".xml" => "xml",
    ".md" => "markdown",
    ".yaml" or ".yml" => "yaml",
    _ => "plaintext"
};
}