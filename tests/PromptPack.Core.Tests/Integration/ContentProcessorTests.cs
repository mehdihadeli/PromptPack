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

            Assert.DoesNotContain("comment", processed.Content);
            Assert.DoesNotContain("\n\n", processed.Content);
            Assert.Contains("class Sample", processed.Content);
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

            Assert.Contains("public class Sample", processed.Content);
            Assert.Contains("public void Run()", processed.Content);
            Assert.Contains("{ ... }", processed.Content);
            Assert.DoesNotContain("Console.WriteLine", processed.Content);
        }
        finally
        {
            directory.Delete(true);
        }
    }
}
