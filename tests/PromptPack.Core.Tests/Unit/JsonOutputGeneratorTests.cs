using System.Text.Json;
using PromptPack.Models;
using PromptPack.Services.OutputGenerators;

namespace PromptPack.Core.Tests.Unit;

public class JsonOutputGeneratorTests
{
    [Fact]
    public void Should_Produce_Structured_Json_With_Line_Numbers()
    {
        var context = new RepositoryContext
        {
            IncludeFileSummary = false,
            IncludeDirectoryStructure = false,
            ShowLineNumbers = true,
            Files = [ProcessedFile.Create("README.md", ".", "first\nsecond")],
        };

        using var document = JsonDocument.Parse(new JsonOutputGenerator().Generate(context));
        var root = document.RootElement;

        root.GetProperty("fileSummary").ValueKind.ShouldBe(JsonValueKind.Null);
        root.GetProperty("directoryStructure").ValueKind.ShouldBe(JsonValueKind.Null);
        root.GetProperty("files")
            .GetProperty("README.md")
            .GetProperty("language")
            .GetString()
            .ShouldBe("markdown");
        root.GetProperty("files")
            .GetProperty("README.md")
            .GetProperty("content")
            .GetString()!
            .ShouldContain("1 | first");
    }
}
