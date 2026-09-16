using Microsoft.Extensions.Logging;
using PromptPack.Services.Interfaces;

namespace PromptPack.Services;

public class FileCollector : IFileCollector
{
    private readonly ILogger<FileCollector> _logger;

    private static readonly string[] DefaultIgnorePatterns =
    {
        "**/node_modules/**",
        "**/.git/**",
        "**/bin/**",
        "**/obj/**",
        "**/dist/**",
        "**/build/**",
        "**/.vs/**",
        "**/.vscode/**",
    };

    public FileCollector(ILogger<FileCollector> logger)
    {
        _logger = logger;
    }

    public async Task<IReadOnlyList<string>> CollectFilesAsync(
        string directory,
        string? includePatterns,
        string? ignorePatterns
    )
    {
        _logger.LogInformation("Collecting files from {Directory}", directory);

        var ignoreSet = new HashSet<string>(DefaultIgnorePatterns);
        var includeSet = new HashSet<string> { "**/*" };

        // Add .gitignore patterns
        await AddGitignorePatternsAsync(directory, ignoreSet);

        // Add custom patterns
        if (!string.IsNullOrEmpty(ignorePatterns))
        {
            foreach (var pattern in ignorePatterns.Split(','))
                ignoreSet.Add(pattern.Trim());
        }

        if (!string.IsNullOrEmpty(includePatterns))
        {
            includeSet.Clear();
            foreach (var pattern in includePatterns.Split(','))
                includeSet.Add(pattern.Trim());
        }

        // Collect files
        var allFiles = Directory.GetFiles(directory, "*.*", SearchOption.AllDirectories);
        var filteredFiles = new List<string>();

        foreach (var file in allFiles)
        {
            var relativePath = Path.GetRelativePath(directory, file).Replace('\\', '/');

            if (ShouldInclude(relativePath, includeSet) && !ShouldIgnore(relativePath, ignoreSet))
            {
                filteredFiles.Add(file);
            }
        }

        _logger.LogInformation(
            "Collected {Count} files from {Directory}",
            filteredFiles.Count,
            directory
        );
        return filteredFiles;
    }

    public async Task<IReadOnlyList<string>> CollectPathsFromStdinAsync(
        string directory,
        string? includePatterns,
        string? ignorePatterns
    )
    {
        var ignoreSet = new HashSet<string>(DefaultIgnorePatterns);
        await AddGitignorePatternsAsync(directory, ignoreSet);
        if (!string.IsNullOrWhiteSpace(ignorePatterns))
            foreach (var pattern in ignorePatterns.Split(','))
                ignoreSet.Add(pattern.Trim());

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
        return Filter(paths, directory, includePatterns, string.Join(',', ignoreSet));
    }

    public async Task<IReadOnlyList<string>> CollectDirectoryPathsAsync(
        string directory,
        string? ignorePatterns
    )
    {
        var ignoreSet = new HashSet<string>(DefaultIgnorePatterns);
        await AddGitignorePatternsAsync(directory, ignoreSet);
        if (!string.IsNullOrWhiteSpace(ignorePatterns))
            foreach (var pattern in ignorePatterns.Split(','))
                ignoreSet.Add(pattern.Trim());

        var paths = Directory
            .GetFiles(directory, "*", SearchOption.AllDirectories)
            .Select(file => Path.GetRelativePath(directory, file).Replace('\\', '/'))
            .Where(path => !ShouldIgnore(path, ignoreSet))
            .ToList();
        return paths;
    }

    private static IReadOnlyList<string> Filter(
        IEnumerable<string> files,
        string directory,
        string? includePatterns,
        string? ignorePatterns
    )
    {
        var includes = string.IsNullOrWhiteSpace(includePatterns)
            ? new HashSet<string> { "**/*" }
            : includePatterns.Split(',').Select(pattern => pattern.Trim()).ToHashSet();
        var ignores = string.IsNullOrWhiteSpace(ignorePatterns)
            ? new HashSet<string>()
            : ignorePatterns.Split(',').Select(pattern => pattern.Trim()).ToHashSet();
        return files
            .Where(file =>
            {
                var relative = Path.GetRelativePath(directory, file).Replace('\\', '/');
                return ShouldInclude(relative, includes) && !ShouldIgnore(relative, ignores);
            })
            .ToList();
    }

    private async Task AddGitignorePatternsAsync(string directory, HashSet<string> ignoreSet)
    {
        var gitignorePath = Path.Combine(directory, ".gitignore");
        if (File.Exists(gitignorePath))
        {
            var lines = await File.ReadAllLinesAsync(gitignorePath);
            foreach (var line in lines)
            {
                var trimmed = line.Trim();
                if (!string.IsNullOrWhiteSpace(trimmed) && !trimmed.StartsWith("#"))
                {
                    ignoreSet.Add(trimmed);
                }
            }
        }
    }

    private static bool ShouldInclude(string path, HashSet<string> patterns)
    {
        return patterns.Any(pattern => MatchGlob(path, pattern));
    }

    private static bool ShouldIgnore(string path, HashSet<string> patterns)
    {
        return patterns.Any(pattern => MatchGlob(path, pattern));
    }

    private static bool MatchGlob(string path, string pattern)
    {
        // Simplified glob matching - would use proper library in production
        var regex = new System.Text.RegularExpressions.Regex(
            "^"
                + System
                    .Text.RegularExpressions.Regex.Escape(pattern)
                    .Replace("\\*\\*/", ".*")
                    .Replace("\\*", "[^/]*")
                    .Replace("\\?", "[^/]")
                + "$"
        );
        return regex.IsMatch(path);
    }
}
