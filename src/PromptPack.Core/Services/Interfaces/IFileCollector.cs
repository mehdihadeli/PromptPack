namespace PromptPack.Services.Interfaces;

public interface IFileCollector
{
    Task<IReadOnlyList<string>> CollectFilesAsync(
        string directory,
        string? includePatterns,
        string? ignorePatterns
    );
    Task<IReadOnlyList<string>> CollectPathsFromStdinAsync(
        string directory,
        string? includePatterns,
        string? ignorePatterns
    );
    Task<IReadOnlyList<string>> CollectDirectoryPathsAsync(
        string directory,
        string? ignorePatterns
    );
}
