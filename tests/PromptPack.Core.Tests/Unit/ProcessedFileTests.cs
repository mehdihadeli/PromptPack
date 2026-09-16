using PromptPack.Models;

namespace PromptPack.Core.Tests.Unit;

public class ProcessedFileTests
{
    [Theory]
    [InlineData("sample.cs", "csharp")]
    [InlineData("sample.ts", "typescript")]
    [InlineData("sample.md", "markdown")]
    [InlineData("sample.unknown", "plaintext")]
    public void Create_InfersLanguageAndNormalizesRelativePath(
        string fileName,
        string expectedLanguage
    )
    {
        var processed = ProcessedFile.Create(
            Path.Combine("repo", "src", fileName),
            Path.Combine("repo"),
            "content"
        );

        Assert.Equal($"src/{fileName}", processed.RelativePath);
        Assert.Equal(expectedLanguage, processed.Language);
        Assert.Equal("content", processed.Content);
        Assert.Equal(7, processed.Size);
    }
}
