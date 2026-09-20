using System.Text.RegularExpressions;
using PromptPack.Models;
using PromptPack.Services.Interfaces;

namespace PromptPack.Services;

public sealed class SecurityScanner : ISecurityScanner
{
    private static readonly (string Rule, Regex Pattern)[] Rules =
    [
        (
            "private key",
            new Regex("-----BEGIN (RSA |EC |OPENSSH )?PRIVATE KEY-----", RegexOptions.Compiled)
        ),
        ("AWS access key", new Regex("\\bAKIA[0-9A-Z]{16}\\b", RegexOptions.Compiled)),
        ("GitHub token", new Regex(@"\bgh[pousr]_[A-Za-z0-9_]{20,}", RegexOptions.Compiled)),
        (
            "generic secret assignment",
            new Regex(
                "(?i)\\b(api[_-]?key|secret|password|token)\\b\\s*[:=]\\s*[\\\"']?[^\\s\\\"']{12,}",
                RegexOptions.Compiled
            )
        ),
    ];

    public async Task<IReadOnlyList<SecurityFinding>> ScanAsync(
        IEnumerable<string> files,
        string baseDirectory
    )
    {
        var findings = new List<SecurityFinding>();
        foreach (var file in files)
        {
            var content = await File.ReadAllTextAsync(file);
            var match = Rules.FirstOrDefault(rule => rule.Pattern.IsMatch(content));
            if (match.Pattern is not null)
                findings.Add(
                    new SecurityFinding(
                        Path.GetRelativePath(baseDirectory, file).Replace('\\', '/'),
                        match.Rule
                    )
                );
        }
        return findings;
    }
}
