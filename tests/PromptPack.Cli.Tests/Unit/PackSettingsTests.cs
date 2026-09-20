using PromptPack.Commands;

namespace PromptPack.Cli.Tests.Unit;

public class PackSettingsTests
{
    [Fact]
    public void Should_Default_To_Current_Directory_And_Markdown_Output()
    {
        var settings = new PackSettings();

        settings.Directory.ShouldBe(".");
        settings.Style.ShouldBe("markdown");
        settings.Output.ShouldBeNull();
        settings.Include.ShouldBeNull();
        settings.Ignore.ShouldBeNull();
        settings.TokenBudget.ShouldBeNull();
        settings.Copy.ShouldBeFalse();
        settings.Stdout.ShouldBeFalse();
        settings.Compress.ShouldBeFalse();
        settings.RemoveComments.ShouldBeFalse();
        settings.RemoveEmptyLines.ShouldBeFalse();
        settings.ShowLineNumbers.ShouldBeFalse();
        settings.NoFileSummary.ShouldBeFalse();
        settings.NoDirectoryStructure.ShouldBeFalse();
        settings.NoFiles.ShouldBeFalse();
        settings.TokenCount.ShouldBeFalse();
        settings.Verbose.ShouldBeFalse();
        settings.Config.ShouldBeNull();
        settings.Stdin.ShouldBeFalse();
        settings.SecurityScan.ShouldBeNull();
        settings.SecurityScanner.ShouldBeNull();
        settings.HeaderText.ShouldBeNull();
        settings.InstructionFilePath.ShouldBeNull();
        settings.IncludeFullDirectoryStructure.ShouldBeFalse();
        settings.SplitOutput.ShouldBeNull();
        settings.Init.ShouldBeFalse();
        settings.NoGitignore.ShouldBeFalse();
        settings.NoDefaultPatterns.ShouldBeFalse();
        settings.Remote.ShouldBeNull();
        settings.RemoteBranch.ShouldBeNull();
    }

    [Fact]
    public void Should_Represent_Complete_Pack_Request()
    {
        var settings = new PackSettings
        {
            Directory = "src",
            Output = "context.json",
            Style = "json",
            Copy = true,
            Stdout = false,
            Include = "**/*.cs",
            Ignore = "**/*.generated.cs",
            Compress = true,
            RemoveComments = true,
            RemoveEmptyLines = true,
            ShowLineNumbers = true,
            NoFileSummary = true,
            NoDirectoryStructure = true,
            NoFiles = false,
            TokenBudget = 10_000,
            TokenCount = true,
            Verbose = true,
            Config = "promptpack.config.yaml",
            Stdin = true,
            SecurityScan = true,
            SecurityScanner = "secretlint",
            HeaderText = "Review this",
            InstructionFilePath = "instructions.md",
            IncludeFullDirectoryStructure = true,
            SplitOutput = "2MB",
            Init = true,
            NoGitignore = true,
            NoDefaultPatterns = true,
            Remote = "https://github.com/org/repo.git",
            RemoteBranch = "main",
        };

        settings.Directory.ShouldBe("src");
        settings.Output.ShouldBe("context.json");
        settings.Style.ShouldBe("json");
        settings.Copy.ShouldBeTrue();
        settings.Include.ShouldBe("**/*.cs");
        settings.Ignore.ShouldBe("**/*.generated.cs");
        settings.Compress.ShouldBeTrue();
        settings.RemoveComments.ShouldBeTrue();
        settings.RemoveEmptyLines.ShouldBeTrue();
        settings.ShowLineNumbers.ShouldBeTrue();
        settings.NoFileSummary.ShouldBeTrue();
        settings.NoDirectoryStructure.ShouldBeTrue();
        settings.TokenBudget.ShouldBe(10_000);
        settings.TokenCount.ShouldBeTrue();
        settings.Verbose.ShouldBeTrue();
        settings.Config.ShouldBe("promptpack.config.yaml");
        settings.Stdin.ShouldBeTrue();
        settings.SecurityScan.ShouldBe(true);
        settings.SecurityScanner.ShouldBe("secretlint");
        settings.HeaderText.ShouldBe("Review this");
        settings.InstructionFilePath.ShouldBe("instructions.md");
        settings.IncludeFullDirectoryStructure.ShouldBeTrue();
        settings.SplitOutput.ShouldBe("2MB");
        settings.Init.ShouldBeTrue();
        settings.NoGitignore.ShouldBeTrue();
        settings.NoDefaultPatterns.ShouldBeTrue();
        settings.Remote.ShouldBe("https://github.com/org/repo.git");
        settings.RemoteBranch.ShouldBe("main");
    }
}
