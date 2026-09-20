using System.Text.Json;
using PromptPack.Models;
using PromptPack.Services.Interfaces;
using YamlDotNet.Serialization;
using YamlDotNet.Serialization.NamingConventions;

namespace PromptPack.Services;

public sealed class ConfigurationLoader : IConfigurationLoader
{
    private static readonly string[] ConfigNames =
    [
        "promptpack.config.json",
        "promptpack.config.yaml",
        "promptpack.config.yml",
    ];

    public async Task<PromptPackConfig> LoadAsync(string directory, string? explicitPath = null)
    {
        var result = new PromptPackConfig();
        var paths = new List<string>();
        var globalDirectory = OperatingSystem.IsWindows()
            ? Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData)
            : Environment.GetEnvironmentVariable("XDG_CONFIG_HOME")
                ?? Path.Combine(
                    Environment.GetFolderPath(Environment.SpecialFolder.UserProfile),
                    ".config"
                );
        foreach (var name in ConfigNames)
        {
            var path = Path.Combine(globalDirectory, "PromptPack", name);
            if (File.Exists(path))
                paths.Add(path);
        }

        var current = new DirectoryInfo(Path.GetFullPath(directory));
        var ancestors = new Stack<string>();
        while (current is not null)
        {
            ancestors.Push(current.FullName);
            current = current.Parent;
        }

        foreach (var ancestor in ancestors)
        foreach (var name in ConfigNames)
        {
            var path = Path.Combine(ancestor, name);
            if (File.Exists(path))
                paths.Add(path);
        }

        if (!string.IsNullOrWhiteSpace(explicitPath))
            paths.Add(Path.GetFullPath(explicitPath, directory));

        foreach (var path in paths.Distinct(StringComparer.OrdinalIgnoreCase))
            Merge(result, await LoadFileAsync(path));

        return result;
    }

    private static async Task<PromptPackConfig> LoadFileAsync(string path)
    {
        var text = await File.ReadAllTextAsync(path);
        if (Path.GetExtension(path).Equals(".json", StringComparison.OrdinalIgnoreCase))
            return JsonSerializer.Deserialize<PromptPackConfig>(
                    text,
                    new JsonSerializerOptions { PropertyNameCaseInsensitive = true }
                ) ?? new();

        return new DeserializerBuilder()
            .WithNamingConvention(CamelCaseNamingConvention.Instance)
            .IgnoreUnmatchedProperties()
            .Build()
            .Deserialize<PromptPackConfig>(text);
    }

    private static void Merge(PromptPackConfig target, PromptPackConfig source)
    {
        target.Include = source.Include ?? target.Include;
        target.Exclude = source.Exclude ?? target.Exclude;
        target.UseGitignore = source.UseGitignore ?? target.UseGitignore;
        target.UseDefaultPatterns = source.UseDefaultPatterns ?? target.UseDefaultPatterns;
        target.Compress = source.Compress ?? target.Compress;
        target.RemoveComments = source.RemoveComments ?? target.RemoveComments;
        target.RemoveEmptyLines = source.RemoveEmptyLines ?? target.RemoveEmptyLines;
        target.IncludeFullDirectoryStructure =
            source.IncludeFullDirectoryStructure ?? target.IncludeFullDirectoryStructure;
        target.SecurityScan = source.SecurityScan ?? target.SecurityScan;
        target.SecurityScanner = source.SecurityScanner ?? target.SecurityScanner;
        target.HeaderText = source.HeaderText ?? target.HeaderText;
        target.InstructionFilePath = source.InstructionFilePath ?? target.InstructionFilePath;
        target.SplitOutput = source.SplitOutput ?? target.SplitOutput;
    }
}
