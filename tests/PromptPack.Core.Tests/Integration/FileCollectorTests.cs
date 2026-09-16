using Microsoft.Extensions.Logging.Abstractions;
using PromptPack.Services;

namespace PromptPack.Core.Tests.Integration;

public class FileCollectorTests
{
    [Fact]
    public async Task CollectFilesAsync_ExcludesDefaultBuildDirectories()
    {
        var directory = Directory.CreateTempSubdirectory("promptpack-tests-");
        try
        {
            Directory.CreateDirectory(Path.Combine(directory.FullName, "bin"));
            Directory.CreateDirectory(Path.Combine(directory.FullName, "src"));
            await File.WriteAllTextAsync(
                Path.Combine(directory.FullName, "bin", "generated.dll"),
                "binary",
                TestContext.Current.CancellationToken
            );
            await File.WriteAllTextAsync(
                Path.Combine(directory.FullName, "src", "Program.cs"),
                "code",
                TestContext.Current.CancellationToken
            );

            var files = await new FileCollector(
                NullLogger<FileCollector>.Instance
            ).CollectFilesAsync(directory.FullName, null, null);

            files
                .Select(file => Path.GetRelativePath(directory.FullName, file).Replace('\\', '/'))
                .ShouldBe(["src/Program.cs"]);
        }
        finally
        {
            directory.Delete(true);
        }
    }

    [Fact]
    public async Task CollectFilesAsync_AppliesIncludeAndIgnorePatterns()
    {
        var directory = Directory.CreateTempSubdirectory("promptpack-tests-");
        try
        {
            Directory.CreateDirectory(Path.Combine(directory.FullName, "src"));
            await File.WriteAllTextAsync(
                Path.Combine(directory.FullName, "README.md"),
                "readme",
                TestContext.Current.CancellationToken
            );
            await File.WriteAllTextAsync(
                Path.Combine(directory.FullName, "src", "Program.cs"),
                "code",
                TestContext.Current.CancellationToken
            );
            await File.WriteAllTextAsync(
                Path.Combine(directory.FullName, "src", "Program.generated.cs"),
                "generated",
                TestContext.Current.CancellationToken
            );

            var files = await new FileCollector(
                NullLogger<FileCollector>.Instance
            ).CollectFilesAsync(directory.FullName, "**/*.cs", "**/*.generated.cs");

            var relativeFiles = files
                .Select(file => Path.GetRelativePath(directory.FullName, file).Replace('\\', '/'))
                .ToArray();

            relativeFiles.ShouldBe(["src/Program.cs"]);
        }
        finally
        {
            directory.Delete(true);
        }
    }

    [Fact]
    public async Task CollectFilesAsync_AppliesGitignorePatterns()
    {
        var directory = Directory.CreateTempSubdirectory("promptpack-tests-");
        try
        {
            Directory.CreateDirectory(Path.Combine(directory.FullName, "docs"));
            await File.WriteAllTextAsync(
                Path.Combine(directory.FullName, ".gitignore"),
                "docs/**\n",
                TestContext.Current.CancellationToken
            );
            await File.WriteAllTextAsync(
                Path.Combine(directory.FullName, "docs", "guide.md"),
                "guide",
                TestContext.Current.CancellationToken
            );
            await File.WriteAllTextAsync(
                Path.Combine(directory.FullName, "README.md"),
                "readme",
                TestContext.Current.CancellationToken
            );

            var files = await new FileCollector(
                NullLogger<FileCollector>.Instance
            ).CollectFilesAsync(directory.FullName, null, null);

            var relativeFiles = files
                .Select(file => Path.GetRelativePath(directory.FullName, file).Replace('\\', '/'))
                .OrderBy(path => path)
                .ToArray();

            relativeFiles.ShouldBe([".gitignore", "README.md"]);
        }
        finally
        {
            directory.Delete(true);
        }
    }
}
