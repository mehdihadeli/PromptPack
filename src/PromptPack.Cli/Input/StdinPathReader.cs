using PromptPack.Services.Interfaces;

namespace PromptPack.Cli.Input;

public sealed class StdinPathReader(IFileCollector fileCollector)
{
    public async Task<IReadOnlyList<string>> ReadAsync(
        string directory,
        string? includePatterns,
        string? ignorePatterns,
        bool useGitignore = true,
        bool useDefaultPatterns = true
    )
    {
        var paths = new List<string>();
        string? line;
        while ((line = await Console.In.ReadLineAsync()) is not null)
        {
            if (string.IsNullOrWhiteSpace(line))
                continue;

            var path = Path.GetFullPath(line.Trim(), directory);
            if (File.Exists(path))
                paths.Add(path);
        }

        return await fileCollector.FilterPathsAsync(
            directory,
            includePatterns,
            ignorePatterns,
            paths,
            useGitignore,
            useDefaultPatterns
        );
    }
}
