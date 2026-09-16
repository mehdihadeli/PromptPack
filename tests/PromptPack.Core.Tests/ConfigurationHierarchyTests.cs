using PromptPack.Services;

namespace PromptPack.Core.Tests;

public sealed class ConfigurationHierarchyTests
{
    [Fact]
    public async Task LoadAsync_MergesAncestorAndExplicitConfigFromBroadToNarrow()
    {
        var root = Directory.CreateTempSubdirectory("promptpack-config-");
        var child = Directory.CreateDirectory(Path.Combine(root.FullName, "child"));
        try
        {
            await File.WriteAllTextAsync(
                Path.Combine(root.FullName, "promptpack.config.json"),
                "{\"include\": \"root/**\", \"securityScan\": true}",
                TestContext.Current.CancellationToken
            );
            await File.WriteAllTextAsync(
                Path.Combine(child.FullName, "promptpack.config.yaml"),
                "include: child/**\nheaderText: Child review\n",
                TestContext.Current.CancellationToken
            );
            var explicitPath = Path.Combine(root.FullName, "explicit.json");
            await File.WriteAllTextAsync(
                explicitPath,
                "{\"headerText\": \"Explicit review\", \"splitOutput\": \"1KB\"}",
                TestContext.Current.CancellationToken
            );

            var config = await new ConfigurationLoader().LoadAsync(child.FullName, explicitPath);

            Assert.Equal("child/**", config.Include);
            Assert.True(config.SecurityScan);
            Assert.Equal("Explicit review", config.HeaderText);
            Assert.Equal("1KB", config.SplitOutput);
        }
        finally
        {
            root.Delete(true);
        }
    }
}
