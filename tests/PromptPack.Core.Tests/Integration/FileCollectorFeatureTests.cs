using Microsoft.Extensions.Logging.Abstractions;
using PromptPack.Services;

namespace PromptPack.Core.Tests.Integration;

public sealed class FileCollectorFeatureTests
{
    [Fact]
    public async Task CollectPathsFromStdinAsync_AppliesIncludesIgnoresAndDefaultDirectories()
    {
        var directory = Directory.CreateTempSubdirectory("promptpack-stdin-");
        var originalInput = Console.In;
        try
        {
            Directory.CreateDirectory(Path.Combine(directory.FullName, "src"));
            Directory.CreateDirectory(Path.Combine(directory.FullName, "bin"));
            await File.WriteAllTextAsync(
                Path.Combine(directory.FullName, "src", "Program.cs"),
                "code",
                TestContext.Current.CancellationToken
            );
            await File.WriteAllTextAsync(
                Path.Combine(directory.FullName, "src", "Notes.txt"),
                "notes",
                TestContext.Current.CancellationToken
            );
            await File.WriteAllTextAsync(
                Path.Combine(directory.FullName, "bin", "generated.cs"),
                "generated",
                TestContext.Current.CancellationToken
            );

            Console.SetIn(
                new StringReader("src/Program.cs\nsrc/Notes.txt\nbin/generated.cs\nmissing.cs\n")
            );
            var files = await new FileCollector(
                NullLogger<FileCollector>.Instance
            ).CollectPathsFromStdinAsync(directory.FullName, "**/*.cs", null);

            files
                .Select(path => Path.GetRelativePath(directory.FullName, path).Replace('\\', '/'))
                .ShouldBe(["src/Program.cs"]);
        }
        finally
        {
            Console.SetIn(originalInput);
            directory.Delete(true);
        }
    }

    [Fact]
    public async Task CollectDirectoryPathsAsync_ReturnsNonIgnoredFullTree()
    {
        var directory = Directory.CreateTempSubdirectory("promptpack-tree-");
        try
        {
            Directory.CreateDirectory(Path.Combine(directory.FullName, "src"));
            Directory.CreateDirectory(Path.Combine(directory.FullName, "bin"));
            await File.WriteAllTextAsync(
                Path.Combine(directory.FullName, "src", "Program.cs"),
                "code",
                TestContext.Current.CancellationToken
            );
            await File.WriteAllTextAsync(
                Path.Combine(directory.FullName, "bin", "generated.dll"),
                "binary",
                TestContext.Current.CancellationToken
            );

            var paths = await new FileCollector(
                NullLogger<FileCollector>.Instance
            ).CollectDirectoryPathsAsync(directory.FullName, null);

            paths.ShouldBe(["src/Program.cs"]);
        }
        finally
        {
            directory.Delete(true);
        }
    }
}
