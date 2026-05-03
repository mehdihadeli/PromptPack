namespace PromptPack.Services.Interfaces;

public interface IFileCollector
{
    Task<IReadOnlyList<string>> CollectFilesAsync(string directory, string? includePatterns, string? ignorePatterns);
}