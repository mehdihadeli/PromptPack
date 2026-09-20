using Microsoft.Extensions.Logging.Abstractions;
using PromptPack.Services;

namespace PromptPack.Core.Tests.Integration;

public sealed class FileCollectorFilterTests
{
    [Fact]
    public async Task Should_Apply_Includes_Ignores_And_Default_Directories()
    {
        var directory = Directory.CreateTempSubdirectory("promptpack-stdin-");
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

            var paths = new[]
            {
                Path.Combine(directory.FullName, "src", "Program.cs"),
                Path.Combine(directory.FullName, "src", "Notes.txt"),
                Path.Combine(directory.FullName, "bin", "generated.cs"),
                Path.Combine(directory.FullName, "missing.cs"),
            };
            var files = await new FileCollector(
                NullLogger<FileCollector>.Instance
            ).FilterPathsAsync(directory.FullName, "**/*.cs", null, paths);

            files
                .Select(path => Path.GetRelativePath(directory.FullName, path).Replace('\\', '/'))
                .ShouldBe(["src/Program.cs"]);
        }
        finally
        {
            directory.Delete(true);
        }
    }

    [Fact]
    public async Task Should_Return_Non_Ignored_Full_Tree()
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
