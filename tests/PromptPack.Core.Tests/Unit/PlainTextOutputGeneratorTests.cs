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

        Assert.Contains("REPOSITORY CONTEXT FOR AI ANALYSIS", output);
        Assert.Contains("DIRECTORY STRUCTURE", output);
        Assert.DoesNotContain("FILES", output);
        Assert.DoesNotContain("FILE: README.md", output);
    }
}
