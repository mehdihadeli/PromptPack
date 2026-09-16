using System.Diagnostics;

namespace PromptPack.Cli.Tests.Integration;

public class CliIntegrationTests
{
    [Fact]
    public async Task PackCommand_SupportsShortOptionAliases()
    {
        var directory = Directory.CreateTempSubdirectory("promptpack-cli-tests-");
        try
        {
            Directory.CreateDirectory(Path.Combine(directory.FullName, "ignored"));
            await File.WriteAllTextAsync(
                Path.Combine(directory.FullName, "README.md"),
                "# Sample",
                TestContext.Current.CancellationToken
            );
            await File.WriteAllTextAsync(
                Path.Combine(directory.FullName, "ignored", "README.md"),
                "# Ignored",
                TestContext.Current.CancellationToken
            );

            using var process = new Process
            {
                StartInfo = new ProcessStartInfo
                {
                    FileName = "dotnet",
                    Arguments =
                        "run --project src/PromptPack.Cli -- pack \""
                        + directory.FullName
                        + "\" -i \"**/*.md,README.md\" -e \"ignored/**\" -s -y json -f -d",
                    WorkingDirectory = FindRepositoryRoot(),
                    RedirectStandardOutput = true,
                    RedirectStandardError = true,
                    UseShellExecute = false,
                },
            };

            process.Start();

            var standardOutput = await process.StandardOutput.ReadToEndAsync(
                TestContext.Current.CancellationToken
            );
            var standardError = await process.StandardError.ReadToEndAsync(
                TestContext.Current.CancellationToken
            );
            await process.WaitForExitAsync(TestContext.Current.CancellationToken);

            process.ExitCode.ShouldBe(0);
            standardOutput.ShouldContain("\"files\"");
            standardOutput.ShouldContain("\"files\": null");
            standardOutput.ShouldNotContain("# Sample");
            standardOutput.ShouldNotContain("AI-Friendly Repository Context Packer");
            standardOutput.ShouldNotContain("Ignored");
            string.IsNullOrWhiteSpace(standardError).ShouldBeTrue(standardError);
        }
        finally
        {
            directory.Delete(true);
        }
    }

    [Fact]
    public async Task PackCommand_WritesJsonToStdout()
    {
        var directory = Directory.CreateTempSubdirectory("promptpack-cli-tests-");
        try
        {
            await File.WriteAllTextAsync(
                Path.Combine(directory.FullName, "README.md"),
                "# Sample",
                TestContext.Current.CancellationToken
            );

            using var process = new Process
            {
                StartInfo = new ProcessStartInfo
                {
                    FileName = "dotnet",
                    Arguments =
                        "run --project src/PromptPack.Cli -- pack \""
                        + directory.FullName
                        + "\" -s -y json -d -m",
                    WorkingDirectory = FindRepositoryRoot(),
                    RedirectStandardOutput = true,
                    RedirectStandardError = true,
                    UseShellExecute = false,
                },
            };

            process.Start();

            var standardOutput = await process.StandardOutput.ReadToEndAsync(
                TestContext.Current.CancellationToken
            );
            var standardError = await process.StandardError.ReadToEndAsync(
                TestContext.Current.CancellationToken
            );
            await process.WaitForExitAsync(TestContext.Current.CancellationToken);

            process.ExitCode.ShouldBe(0);
            standardOutput.ShouldContain("\"files\"");
            standardOutput.ShouldContain("README.md");
            standardOutput.ShouldNotContain("Pack Complete");
            string.IsNullOrWhiteSpace(standardError)
                .ShouldBeTrue($"Expected no stderr output, got: {standardError}");
        }
        finally
        {
            directory.Delete(true);
        }
    }

    [Fact]
    public async Task PackCommand_SupportsMetadataSecurityAndStdinOptions()
    {
        var directory = Directory.CreateTempSubdirectory("promptpack-cli-tests-");
        try
        {
            await File.WriteAllTextAsync(
                Path.Combine(directory.FullName, "selected.txt"),
                "api_key: long-enough-secret-value",
                TestContext.Current.CancellationToken
            );
            await File.WriteAllTextAsync(
                Path.Combine(directory.FullName, "not-selected.txt"),
                "other",
                TestContext.Current.CancellationToken
            );
            var instructionPath = Path.Combine(directory.FullName, "instructions.md");
            await File.WriteAllTextAsync(
                instructionPath,
                "Inspect security implications.",
                TestContext.Current.CancellationToken
            );

            var result = await RunCliAsync(
                directory.FullName,
                "pack \""
                    + directory.FullName
                    + "\" --stdin --stdout --style json --security-scan --header-text \"Review header\" --instruction-file-path \""
                    + instructionPath
                    + "\"",
                "selected.txt\n"
            );

            result.ExitCode.ShouldBe(0);
            result.StandardOutput.ShouldContain("Review header");
            result.StandardOutput.ShouldContain("Inspect security implications.");
            result.StandardOutput.ShouldContain("generic secret assignment");
            result.StandardOutput.ShouldContain("selected.txt");
            result.StandardOutput.ShouldNotContain("not-selected.txt");
        }
        finally
        {
            directory.Delete(true);
        }
    }

    [Fact]
    public async Task PackCommand_SplitsFileOutputIntoNumberedParts()
    {
        var directory = Directory.CreateTempSubdirectory("promptpack-cli-tests-");
        try
        {
            await File.WriteAllTextAsync(
                Path.Combine(directory.FullName, "README.md"),
                new string('é', 500),
                TestContext.Current.CancellationToken
            );
            var outputPath = Path.Combine(directory.FullName, "context.txt");

            var result = await RunCliAsync(
                directory.FullName,
                "pack \""
                    + directory.FullName
                    + "\" --style plain --split-output 128 --output \""
                    + outputPath
                    + "\""
            );

            result.ExitCode.ShouldBe(0);
            var parts = Directory
                .GetFiles(directory.FullName, "context.part*.txt")
                .OrderBy(path => path)
                .ToArray();
            (parts.Length > 1).ShouldBeTrue();
            Path.GetFileName(parts[0]).ShouldBe("context.part001.txt");
            parts.ShouldAllBe(part => new FileInfo(part).Length <= 128, "part");
        }
        finally
        {
            directory.Delete(true);
        }
    }

    private static string FindRepositoryRoot()
    {
        var directory = new DirectoryInfo(AppContext.BaseDirectory);

        while (directory is not null)
        {
            if (File.Exists(Path.Combine(directory.FullName, "PromptPack.slnx")))
                return directory.FullName;

            directory = directory.Parent;
        }

        throw new DirectoryNotFoundException("Could not locate PromptPack.slnx.");
    }

    private static async Task<CliResult> RunCliAsync(
        string directory,
        string arguments,
        string? standardInput = null
    )
    {
        using var process = new Process
        {
            StartInfo = new ProcessStartInfo
            {
                FileName = "dotnet",
                Arguments = "run --project src/PromptPack.Cli -- " + arguments,
                WorkingDirectory = FindRepositoryRoot(),
                RedirectStandardInput = true,
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                UseShellExecute = false,
            },
        };

        process.Start();
        if (standardInput is not null)
        {
            await process.StandardInput.WriteAsync(standardInput);
            process.StandardInput.Close();
        }
        else
        {
            process.StandardInput.Close();
        }

        var standardOutput = await process.StandardOutput.ReadToEndAsync(
            TestContext.Current.CancellationToken
        );
        var standardError = await process.StandardError.ReadToEndAsync(
            TestContext.Current.CancellationToken
        );
        await process.WaitForExitAsync(TestContext.Current.CancellationToken);
        return new CliResult(process.ExitCode, standardOutput, standardError);
    }

    private sealed record CliResult(int ExitCode, string StandardOutput, string StandardError);
}
