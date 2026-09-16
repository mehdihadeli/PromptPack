using PromptPack.Models;

namespace PromptPack.Services.Interfaces;

public interface IConfigurationLoader
{
    Task<PromptPackConfig> LoadAsync(string directory, string? explicitPath = null);
}
