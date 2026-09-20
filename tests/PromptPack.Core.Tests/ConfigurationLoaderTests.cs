using PromptPack.Services;

namespace PromptPack.Core.Tests;

public sealed class ConfigurationLoaderTests
{
    [Fact]
    public async Task Should_Load_Yaml_Config()
    {
        var cancellationToken = TestContext.Current.CancellationToken;
        var directory = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(directory);
        try
        {
            await File.WriteAllTextAsync(
                Path.Combine(directory, "promptpack.config.yaml"),
                "include: src/**\nsecurityScan: true\nsecurityScanner: secretlint\n",
                cancellationToken
            );
            var config = await new ConfigurationLoader().LoadAsync(directory);
            config.Include.ShouldBe("src/**");
            config.SecurityScan.ShouldBe(true);
            config.SecurityScanner.ShouldBe("secretlint");
        }
        finally
        {
            Directory.Delete(directory, true);
        }
    }

    [Fact]
    public async Task Should_Allow_Explicit_Config_To_Override_Discovered_Config()
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
            config.Include.ShouldBe("custom/**");
        }
        finally
        {
            Directory.Delete(directory, true);
        }
    }
}
