namespace PromptPack.Services.Interfaces;

public interface IFileCollector
{
    Task<IReadOnlyList<string>> CollectFilesAsync(
        string directory,
        string? includePatterns,
        string? ignorePatterns,
        bool useGitignore = true,
        bool useDefaultPatterns = true
    );
    Task<IReadOnlyList<string>> FilterPathsAsync(
        string directory,
        string? includePatterns,
        string? ignorePatterns,
        IEnumerable<string> paths,
        bool useGitignore = true,
        bool useDefaultPatterns = true
    );
    Task<IReadOnlyList<string>> CollectDirectoryPathsAsync(
        string directory,
        string? ignorePatterns,
        bool useGitignore = true,
        bool useDefaultPatterns = true
    );
}
