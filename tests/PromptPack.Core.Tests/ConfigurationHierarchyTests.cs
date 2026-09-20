using PromptPack.Services;

namespace PromptPack.Core.Tests;

public sealed class ConfigurationHierarchyTests
{
    [Fact]
    public async Task Should_Merge_Ancestor_And_Explicit_Config_From_Broad_To_Narrow()
    {
        var root = Directory.CreateTempSubdirectory("promptpack-config-");
        var child = Directory.CreateDirectory(Path.Combine(root.FullName, "child"));
        try
        {
            await File.WriteAllTextAsync(
                Path.Combine(root.FullName, "promptpack.config.json"),
                "{\"include\": \"root/**\", \"securityScan\": true, \"securityScanner\": \"secretlint\"}",
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

            config.Include.ShouldBe("child/**");
            config.SecurityScan.ShouldBe(true);
            config.SecurityScanner.ShouldBe("secretlint");
            config.HeaderText.ShouldBe("Explicit review");
            config.SplitOutput.ShouldBe("1KB");
        }
        finally
        {
            root.Delete(true);
        }
    }
}
