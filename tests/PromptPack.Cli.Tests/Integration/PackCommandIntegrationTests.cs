using System.Diagnostics;

namespace PromptPack.Cli.Tests.Integration;

public class PackCommandIntegrationTests
{
    [Fact]
    public async Task Should_Show_Help_With_Unique_BuiltIn_Short_Options()
    {
        var result = await RunCliAsync(".", "--help");

        result.ExitCode.ShouldBe(0);
        result.StandardOutput.ShouldContain("-h, --help");
        result.StandardOutput.ShouldContain("-v, --version");
        result.StandardOutput.ShouldContain("-j, --header-text");
        result.StandardOutput.ShouldContain("-w, --verbose");
        result.StandardOutput.ShouldContain("--security-scanner");
        (result.StandardOutput.Split("-h,", StringSplitOptions.None).Length - 1).ShouldBe(1);
        (result.StandardOutput.Split("-v,", StringSplitOptions.None).Length - 1).ShouldBe(1);
    }

    [Fact]
    public async Task Should_Support_Short_Option_Aliases()
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
                        "run --project src/PromptPack.Cli -- \""
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
    public async Task Should_Write_Json_To_Stdout()
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
                        "run --project src/PromptPack.Cli -- \""
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

#pragma warning disable xUnit1051
    [Fact]
    public async Task Should_Copy_Default_And_Explicit_Copy_Output_To_Clipboard()
    {
        const string clipboardSentinel = "PromptPack clipboard test sentinel";
        var directory = Directory.CreateTempSubdirectory("promptpack-clipboard-tests-");
        string? originalClipboard = null;
        var outputPath = Path.Combine(
            Path.GetTempPath(),
            $"promptpack-clipboard-output-{Guid.NewGuid():N}.txt"
        );

        try
        {
            try
            {
                originalClipboard = await TextCopy.ClipboardService.GetTextAsync();
                await TextCopy.ClipboardService.SetTextAsync(clipboardSentinel);
            }
            catch (Exception exception)
            {
                Assert.Skip(
                    $"Clipboard is unavailable in this test environment: {exception.Message}"
                );
            }

            const string marker = "clipboard output marker";
            await File.WriteAllTextAsync(
                Path.Combine(directory.FullName, "README.md"),
                marker,
                TestContext.Current.CancellationToken
            );

            var defaultResult = await RunCliAsync(
                directory.FullName,
                $"\"{directory.FullName}\" --style plain --no-file-summary --no-directory-structure"
            );

            defaultResult.ExitCode.ShouldBe(0);
            var defaultClipboard = await TextCopy.ClipboardService.GetTextAsync();
            defaultClipboard.ShouldNotBeNull();
            defaultClipboard!.ShouldContain(marker);

            await TextCopy.ClipboardService.SetTextAsync(clipboardSentinel);
            var fileOnlyResult = await RunCliAsync(
                directory.FullName,
                $"\"{directory.FullName}\" --style plain --no-file-summary --no-directory-structure --output \"{outputPath}\""
            );

            fileOnlyResult.ExitCode.ShouldBe(0);
            var fileOutput = await File.ReadAllTextAsync(
                outputPath,
                TestContext.Current.CancellationToken
            );
            (await TextCopy.ClipboardService.GetTextAsync()).ShouldBe(clipboardSentinel);

            var explicitCopyResult = await RunCliAsync(
                directory.FullName,
                $"\"{directory.FullName}\" --style plain --no-file-summary --no-directory-structure --output \"{outputPath}\" --copy"
            );

            explicitCopyResult.ExitCode.ShouldBe(0);
            var copiedFileOutput = await TextCopy.ClipboardService.GetTextAsync();
            copiedFileOutput.ShouldBe(fileOutput);

            var stdoutResult = await RunCliAsync(
                directory.FullName,
                $"\"{directory.FullName}\" --style plain --no-file-summary --no-directory-structure --stdout"
            );

            stdoutResult.ExitCode.ShouldBe(0);
            var stdoutClipboard = await TextCopy.ClipboardService.GetTextAsync();
            stdoutClipboard.ShouldBe(fileOutput);
        }
        finally
        {
            if (originalClipboard is not null)
            {
                try
                {
                    await TextCopy.ClipboardService.SetTextAsync(originalClipboard);
                }
                catch { }
            }

            directory.Delete(true);
            if (File.Exists(outputPath))
                File.Delete(outputPath);
        }
    }
#pragma warning restore xUnit1051

    [Fact]
    public async Task Should_Support_Metadata_Security_And_Stdin_Options()
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
                "\""
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
    public async Task Should_Run_BuiltIn_Security_Scan_By_Default()
    {
        var directory = Directory.CreateTempSubdirectory("promptpack-cli-tests-");
        try
        {
            await File.WriteAllTextAsync(
                Path.Combine(directory.FullName, "secrets.txt"),
                "api_key: long-enough-secret-value",
                TestContext.Current.CancellationToken
            );

            var result = await RunCliAsync(
                directory.FullName,
                $"\"{directory.FullName}\" --stdout --style json --no-files"
            );

            result.ExitCode.ShouldBe(0);
            result.StandardOutput.ShouldContain("generic secret assignment");
        }
        finally
        {
            directory.Delete(true);
        }
    }

    [Fact]
    public async Task Should_Use_Secretlint_Scanner_When_Selected()
    {
        if (!IsNpxAvailable())
            Assert.Skip("Secretlint integration test requires Node.js/npm npx.");

        var directory = Directory.CreateTempSubdirectory("promptpack-cli-tests-");
        try
        {
            await File.WriteAllTextAsync(
                Path.Combine(directory.FullName, "secrets.txt"),
                "aws: AKIA1234567890ABCD",
                TestContext.Current.CancellationToken
            );

            var result = await RunCliAsync(
                directory.FullName,
                $"\"{directory.FullName}\" --security-scanner secretlint --stdout --style json --no-files"
            );

            result.ExitCode.ShouldBe(0);
            result.StandardOutput.ShouldContain("securityFindings");
            result.StandardOutput.ShouldContain("secrets.txt");
        }
        finally
        {
            directory.Delete(true);
        }
    }

    [Fact]
    public async Task Should_Allow_Configuration_To_Disable_Default_Security_Scan()
    {
        var directory = Directory.CreateTempSubdirectory("promptpack-cli-tests-");
        try
        {
            await File.WriteAllTextAsync(
                Path.Combine(directory.FullName, "promptpack.config.json"),
                "{\"securityScan\":false}",
                TestContext.Current.CancellationToken
            );
            await File.WriteAllTextAsync(
                Path.Combine(directory.FullName, "secrets.txt"),
                "api_key: long-enough-secret-value",
                TestContext.Current.CancellationToken
            );

            var result = await RunCliAsync(
                directory.FullName,
                $"\"{directory.FullName}\" --stdout --style json --no-files"
            );

            result.ExitCode.ShouldBe(0);
            result.StandardOutput.ShouldNotContain("generic secret assignment");
        }
        finally
        {
            directory.Delete(true);
        }
    }

    [Fact]
    public async Task Should_Reject_Unknown_Security_Scanner()
    {
        var directory = Directory.CreateTempSubdirectory("promptpack-cli-tests-");
        try
        {
            await File.WriteAllTextAsync(
                Path.Combine(directory.FullName, "README.md"),
                "# Sample",
                TestContext.Current.CancellationToken
            );

            var result = await RunCliAsync(
                directory.FullName,
                $"\"{directory.FullName}\" --security-scanner unknown"
            );

            result.ExitCode.ShouldBe(1);
            (result.StandardOutput + result.StandardError).ShouldContain("built-in");
            (result.StandardOutput + result.StandardError).ShouldContain("secretlint");
        }
        finally
        {
            directory.Delete(true);
        }
    }

    [Fact]
    public async Task Should_Write_BuiltIn_Security_Scanner_Default_To_Initialized_Config()
    {
        var directory = Directory.CreateTempSubdirectory("promptpack-cli-tests-");
        try
        {
            var result = await RunCliAsync(directory.FullName, $"\"{directory.FullName}\" --init");

            result.ExitCode.ShouldBe(0);
            var config = await File.ReadAllTextAsync(
                Path.Combine(directory.FullName, "promptpack.config.json"),
                TestContext.Current.CancellationToken
            );
            config.ShouldContain("\"securityScan\": true");
            config.ShouldContain("\"securityScanner\": \"built-in\"");
        }
        finally
        {
            directory.Delete(true);
        }
    }

    [Fact]
    public async Task Should_Split_File_Output_Into_Numbered_Parts()
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
                "\""
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

    private static bool IsNpxAvailable()
    {
        using var process = new Process
        {
            StartInfo = new ProcessStartInfo
            {
                FileName = OperatingSystem.IsWindows() ? "npx.cmd" : "npx",
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                UseShellExecute = false,
                CreateNoWindow = true,
            },
        };
        process.StartInfo.ArgumentList.Add("--version");

        try
        {
            process.Start();
            process.WaitForExit();
            return process.ExitCode == 0;
        }
        catch (System.ComponentModel.Win32Exception)
        {
            return false;
        }
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
