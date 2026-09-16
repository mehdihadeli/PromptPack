using PromptPack.Services;

namespace PromptPack.Core.Tests;

public sealed class ConfigurationLoaderTests
{
    [Fact]
    public async Task LoadsYamlConfig()
    {
        var cancellationToken = TestContext.Current.CancellationToken;
        var directory = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(directory);
        try
        {
            await File.WriteAllTextAsync(
                Path.Combine(directory, "promptpack.config.yaml"),
                "include: src/**\nsecurityScan: true\n",
                cancellationToken
            );
            var config = await new ConfigurationLoader().LoadAsync(directory);
            Assert.Equal("src/**", config.Include);
            Assert.True(config.SecurityScan);
        }
        finally
        {
            Directory.Delete(directory, true);
        }
    }

    [Fact]
    public async Task ExplicitConfigOverridesDiscoveredConfig()
    {
        var cancellationToken = TestContext.Current.CancellationToken;
        var directory = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(directory);
        try
        {
            await File.WriteAllTextAsync(
                Path.Combine(directory, "promptpack.config.json"),
                "{\"include\": \"root/**\"}",
                cancellationToken
            );
            var explicitPath = Path.Combine(directory, "custom.json");
            await File.WriteAllTextAsync(
                explicitPath,
                "{\"include\": \"custom/**\"}",
                cancellationToken
            );
            var config = await new ConfigurationLoader().LoadAsync(directory, explicitPath);
            Assert.Equal("custom/**", config.Include);
        }
        finally
        {
            Directory.Delete(directory, true);
        }
    }
}
