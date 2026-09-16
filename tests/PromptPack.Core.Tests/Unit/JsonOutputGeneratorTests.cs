using System.Text.Json;
using PromptPack.Models;
using PromptPack.Services.OutputGenerators;

namespace PromptPack.Core.Tests.Unit;

public class JsonOutputGeneratorTests
{
    [Fact]
    public void Generate_ProducesStructuredJsonWithLineNumbers()
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

        Assert.Equal(JsonValueKind.Null, root.GetProperty("fileSummary").ValueKind);
        Assert.Equal(JsonValueKind.Null, root.GetProperty("directoryStructure").ValueKind);
        Assert.Equal(
            "markdown",
            root.GetProperty("files").GetProperty("README.md").GetProperty("language").GetString()
        );
        Assert.Contains(
            "1 | first",
            root.GetProperty("files").GetProperty("README.md").GetProperty("content").GetString()
        );
    }
}
