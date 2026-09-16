using Microsoft.Extensions.Logging.Abstractions;
using PromptPack.Services;

namespace PromptPack.Core.Tests.Integration;

public class ContentProcessorTests
{
    [Fact]
    public async Task ProcessFileAsync_RemovesCommentsAndEmptyLines()
    {
        var directory = Directory.CreateTempSubdirectory("promptpack-tests-");
        try
        {
            var filePath = Path.Combine(directory.FullName, "sample.cs");
            await File.WriteAllTextAsync(
                filePath,
                "// comment\nclass Sample\n\n{\n    // detail\n}\n",
                TestContext.Current.CancellationToken
            );

            var processed = await new ContentProcessor(
                NullLogger<ContentProcessor>.Instance
            ).ProcessFileAsync(filePath, directory.FullName, true, true, false);

            processed.Content.ShouldNotContain("comment");
            processed.Content.ShouldNotContain("\n\n");
            processed.Content.ShouldContain("class Sample");
        }
        finally
        {
            directory.Delete(true);
        }
    }

    [Fact]
    public async Task ProcessFileAsync_CompressesMethodBodiesToStructuralSummary()
    {
        var directory = Directory.CreateTempSubdirectory("promptpack-tests-");
        try
        {
            var filePath = Path.Combine(directory.FullName, "sample.cs");
            await File.WriteAllTextAsync(
                filePath,
                "public class Sample\n{\n    public void Run() {\n        Console.WriteLine(\"hello\");\n    }\n}\n",
                TestContext.Current.CancellationToken
            );

            var processed = await new ContentProcessor(
                NullLogger<ContentProcessor>.Instance
            ).ProcessFileAsync(filePath, directory.FullName, false, false, true);

            processed.Content.ShouldContain("public class Sample");
            processed.Content.ShouldContain("public void Run()");
            processed.Content.ShouldContain("{ ... }");
            processed.Content.ShouldNotContain("Console.WriteLine");
        }
        finally
        {
            directory.Delete(true);
        }
    }
}
