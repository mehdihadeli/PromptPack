using System.Text.Json;
using PromptPack.Models;
using PromptPack.Services.Interfaces;

namespace PromptPack.Cli.Security;

public sealed class SecretlintSecurityScanner : ISecurityScanner
{
    private readonly ISecretlintProcessRunner _processRunner;

    public SecretlintSecurityScanner(ISecretlintProcessRunner processRunner)
    {
        _processRunner = processRunner;
    }

    public async Task<IReadOnlyList<SecurityFinding>> ScanAsync(
        IEnumerable<string> files,
        string baseDirectory
    )
    {
        var fileList = files.ToArray();
        var result = await _processRunner.RunAsync(fileList, baseDirectory);
        if (result.ExitCode > 1)
        {
            throw new InvalidOperationException(
                $"Secretlint failed with exit code {result.ExitCode}: {result.StandardError.Trim()}"
            );
        }

        if (string.IsNullOrWhiteSpace(result.StandardOutput))
            return Array.Empty<SecurityFinding>();

        try
        {
            using var document = JsonDocument.Parse(result.StandardOutput);
            var findings = new List<SecurityFinding>();
            AddFindings(document.RootElement, baseDirectory, findings);
            return findings;
        }
        catch (JsonException exception)
        {
            throw new InvalidOperationException(
                "Secretlint returned invalid JSON. Ensure the installed Secretlint version supports --format=json.",
                exception
            );
        }
    }

    private static void AddFindings(
        JsonElement element,
        string baseDirectory,
        ICollection<SecurityFinding> findings,
        string? filePath = null
    )
    {
        if (element.ValueKind == JsonValueKind.Object)
        {
            var currentPath =
                GetString(element, "filePath") ?? GetString(element, "file") ?? filePath;

            if (element.TryGetProperty("messages", out var messages))
            {
                foreach (var message in messages.EnumerateArray())
                {
                    var rule =
                        GetString(message, "ruleId")
                        ?? GetString(message, "messageId")
                        ?? GetString(message, "rule")
                        ?? "Secretlint finding";
                    if (currentPath is not null)
                        findings.Add(
                            new SecurityFinding(ToRelativePath(currentPath, baseDirectory), rule)
                        );
                }
            }

            foreach (var property in element.EnumerateObject())
            {
                if (property.Name is not "messages" and not "filePath" and not "file")
                    AddFindings(property.Value, baseDirectory, findings, currentPath);
            }
        }
        else if (element.ValueKind == JsonValueKind.Array)
        {
            foreach (var item in element.EnumerateArray())
                AddFindings(item, baseDirectory, findings, filePath);
        }
    }

    private static string? GetString(JsonElement element, string propertyName) =>
        element.TryGetProperty(propertyName, out var property)
        && property.ValueKind == JsonValueKind.String
            ? property.GetString()
            : null;

    private static string ToRelativePath(string path, string baseDirectory)
    {
        var fullPath = Path.IsPathRooted(path) ? path : Path.GetFullPath(path, baseDirectory);
        return Path.GetRelativePath(baseDirectory, fullPath).Replace('\\', '/');
    }
}
