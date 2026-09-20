using PromptPack.Models;

namespace PromptPack.Services.Interfaces;

public interface ISecurityScanner
{
    Task<IReadOnlyList<SecurityFinding>> ScanAsync(IEnumerable<string> files, string baseDirectory);
}
