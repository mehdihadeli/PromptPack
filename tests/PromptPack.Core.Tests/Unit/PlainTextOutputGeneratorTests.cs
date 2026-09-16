using PromptPack.Models;
using PromptPack.Services.OutputGenerators;

namespace PromptPack.Core.Tests.Unit;

public class PlainTextOutputGeneratorTests
{
    [Fact]
    public void Generate_WhenFilesAreExcludedOmitsFileSection()
    {
        var context = new RepositoryContext
        {
            IncludeFiles = false,
            Files = [ProcessedFile.Create("README.md", ".", "content")],
        };

        var output = new PlainTextOutputGenerator().Generate(context);

        output.ShouldContain("REPOSITORY CONTEXT FOR AI ANALYSIS");
        output.ShouldContain("DIRECTORY STRUCTURE");
        output.Contains("FILES", StringComparison.Ordinal).ShouldBeFalse();
        output.ShouldNotContain("FILE: README.md");
    }
}
