using PromptPack.Commands;

namespace PromptPack.Cli.Tests.Unit;

public class PackSettingsTests
{
    [Fact]
    public void Defaults_TargetCurrentDirectoryAndXmlOutput()
    {
        var settings = new PackSettings();

        Assert.Equal(".", settings.Directory);
        Assert.Equal("xml", settings.Style);
        Assert.Null(settings.Output);
        Assert.Null(settings.Include);
        Assert.Null(settings.Ignore);
        Assert.Null(settings.TokenBudget);
        Assert.False(settings.Copy);
        Assert.False(settings.Stdout);
        Assert.False(settings.Compress);
        Assert.False(settings.RemoveComments);
        Assert.False(settings.RemoveEmptyLines);
        Assert.False(settings.ShowLineNumbers);
        Assert.False(settings.NoFileSummary);
        Assert.False(settings.NoDirectoryStructure);
        Assert.False(settings.NoFiles);
        Assert.False(settings.TokenCount);
        Assert.False(settings.Verbose);
        Assert.Null(settings.Config);
        Assert.False(settings.Stdin);
        Assert.False(settings.SecurityScan);
        Assert.Null(settings.HeaderText);
        Assert.Null(settings.InstructionFilePath);
        Assert.False(settings.IncludeFullDirectoryStructure);
        Assert.Null(settings.SplitOutput);
    }

    [Fact]
    public void PropertiesRepresentCompletePackRequest()
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
            HeaderText = "Review this",
            InstructionFilePath = "instructions.md",
            IncludeFullDirectoryStructure = true,
            SplitOutput = "2MB",
        };

        Assert.Equal("src", settings.Directory);
        Assert.Equal("context.json", settings.Output);
        Assert.Equal("json", settings.Style);
        Assert.True(settings.Copy);
        Assert.Equal("**/*.cs", settings.Include);
        Assert.Equal("**/*.generated.cs", settings.Ignore);
        Assert.True(settings.Compress);
        Assert.True(settings.RemoveComments);
        Assert.True(settings.RemoveEmptyLines);
        Assert.True(settings.ShowLineNumbers);
        Assert.True(settings.NoFileSummary);
        Assert.True(settings.NoDirectoryStructure);
        Assert.Equal(10_000, settings.TokenBudget);
        Assert.True(settings.TokenCount);
        Assert.True(settings.Verbose);
        Assert.Equal("promptpack.config.yaml", settings.Config);
        Assert.True(settings.Stdin);
        Assert.True(settings.SecurityScan);
        Assert.Equal("Review this", settings.HeaderText);
        Assert.Equal("instructions.md", settings.InstructionFilePath);
        Assert.True(settings.IncludeFullDirectoryStructure);
        Assert.Equal("2MB", settings.SplitOutput);
    }
}
