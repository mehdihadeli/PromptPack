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

        processed.RelativePath.ShouldBe($"src/{fileName}");
        processed.Language.ShouldBe(expectedLanguage);
        processed.Content.ShouldBe("content");
        processed.Size.ShouldBe(7);
    }
}
