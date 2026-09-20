using System.Diagnostics;
using System.Text;
using Microsoft.Extensions.Logging;
using PromptPack.Cli.Input;
using PromptPack.Cli.Platform;
using PromptPack.Cli.Security;
using PromptPack.Extensions;
using PromptPack.Models;
using PromptPack.Services;
using PromptPack.Services.Interfaces;
using Spectre.Console;
using Spectre.Console.Cli;

namespace PromptPack.Commands;

public sealed class PackCommand(
    IFileCollector fileCollector,
    IContentProcessor contentProcessor,
    IEnumerable<IOutputGenerator> outputGenerators,
    IClipboardService clipboardService,
    StdinPathReader stdinPathReader,
    TokenEstimator tokenEstimator,
    IConfigurationLoader configurationLoader,
    ISecurityScanner securityScanner,
    SecretlintSecurityScanner secretlintSecurityScanner,
    ILogger<PackCommand> logger
) : AsyncCommand<PackSettings>
{
    public override async Task<int> ExecuteAsync(CommandContext _, PackSettings options)
    {
        if (options.Init)
            return await InitializeConfigAsync(options.Directory);

        var originalDirectory = options.Directory;
        string? temporaryDirectory = null;
        if (!string.IsNullOrWhiteSpace(options.Remote))
        {
            temporaryDirectory = await CloneRemoteAsync(options.Remote, options.RemoteBranch);
            options.Directory = temporaryDirectory;
        }

        string? stdoutOutput = null;
        var originalOutput = Console.Out;
        if (options.Stdout)
            Console.SetOut(TextWriter.Null);

        try
        {
            await AnsiConsole
                .Status()
                .StartAsync(
                    "Packing repository...",
                    async ctx =>
                    {
                        ctx.Status("Loading configuration...");
                        var config = await configurationLoader.LoadAsync(
                            options.Directory,
                            options.Config
                        );
                        options.Include ??= config.Include;
                        options.Ignore ??= config.Exclude;
                        options.HeaderText ??= config.HeaderText;
                        options.InstructionFilePath ??= config.InstructionFilePath;
                        options.SplitOutput ??= config.SplitOutput;
                        options.SecurityScanner ??= config.SecurityScanner ?? "built-in";
                        options.Compress |= config.Compress ?? false;
                        options.RemoveComments |= config.RemoveComments ?? false;
                        options.RemoveEmptyLines |= config.RemoveEmptyLines ?? false;
                        options.SecurityScan ??= config.SecurityScan ?? true;
                        options.NoGitignore |= config.UseGitignore is false;
                        options.NoDefaultPatterns |= config.UseDefaultPatterns is false;
                        options.IncludeFullDirectoryStructure |=
                            config.IncludeFullDirectoryStructure ?? false;

                        ctx.Status("Collecting matching files...");

                        // Collect files
                        var files = options.Stdin
                            ? await stdinPathReader.ReadAsync(
                                options.Directory,
                                options.Include,
                                options.Ignore,
                                !options.NoGitignore,
                                !options.NoDefaultPatterns
                            )
                            : await fileCollector.CollectFilesAsync(
                                options.Directory,
                                options.Include,
                                options.Ignore,
                                !options.NoGitignore,
                                !options.NoDefaultPatterns
                            );

                        if (files.Count == 0)
                        {
                            SpectreExtensions.WriteWarning(
                                "No files matched the current include and exclude rules."
                            );
                            return;
                        }

                        // Process files
                        ctx.Status($"Processing {files.Count:N0} files...");
                        var processedFiles = new List<ProcessedFile>();

                        foreach (var file in files)
                        {
                            var processedFile = await contentProcessor.ProcessFileAsync(
                                file,
                                options.Directory,
                                options.RemoveComments,
                                options.RemoveEmptyLines,
                                options.Compress
                            );
                            processedFiles.Add(processedFile);
                        }

                        ctx.Status(
                            options.SecurityScan == true
                                ? $"Scanning {files.Count:N0} files for likely secrets..."
                                : "Skipping security scan..."
                        );
                        var findings =
                            options.SecurityScan == true
                                ? await GetSecurityScanner(
                                        options.SecurityScanner,
                                        securityScanner,
                                        secretlintSecurityScanner
                                    )
                                    .ScanAsync(files, options.Directory)
                                : Array.Empty<SecurityFinding>();
                        var instructions = options.InstructionFilePath is null
                            ? null
                            : await File.ReadAllTextAsync(
                                Path.GetFullPath(options.InstructionFilePath, options.Directory)
                            );
                        ctx.Status(
                            options.IncludeFullDirectoryStructure
                                ? "Building the full repository tree..."
                                : "Preparing repository context..."
                        );
                        var fullTree = options.IncludeFullDirectoryStructure
                            ? await fileCollector.CollectDirectoryPathsAsync(
                                options.Directory,
                                options.Ignore,
                                !options.NoGitignore,
                                !options.NoDefaultPatterns
                            )
                            : Array.Empty<string>();

                        // Create repository context
                        var repositoryContext = new RepositoryContext
                        {
                            BaseDirectory = options.Directory,
                            Files = processedFiles,
                            GeneratedAt = DateTime.UtcNow,
                            IncludeFileSummary = !options.NoFileSummary,
                            IncludeDirectoryStructure = !options.NoDirectoryStructure,
                            IncludeFiles = !options.NoFiles,
                            ShowLineNumbers = options.ShowLineNumbers,
                            HeaderText = options.HeaderText,
                            Instructions = instructions,
                            FullDirectoryPaths = fullTree,
                            SecurityFindings = findings,
                        };

                        // Generate output
                        ctx.Status($"Generating {options.Style.ToUpperInvariant()} output...");
                        var generator = outputGenerators.First(g =>
                            g.Style == options.Style.ToLower()
                        );
                        var output = generator.Generate(repositoryContext);

                        var totalTokens = tokenEstimator.EstimateTokens(output);
                        if (options.TokenBudget is { } tokenBudget && totalTokens > tokenBudget)
                        {
                            SpectreExtensions.WriteWarning(
                                $"Token budget exceeded: {totalTokens:N0} estimated tokens > {tokenBudget:N0} configured"
                            );
                            Environment.ExitCode = 2;
                        }

                        // Handle output destinations
                        if (options.Stdout)
                        {
                            stdoutOutput = output;
                        }
                        else if (!string.IsNullOrWhiteSpace(options.Output))
                        {
                            var splitSize = ParseSize(options.SplitOutput);
                            if (splitSize is null)
                                await File.WriteAllTextAsync(options.Output, output);
                            else
                            {
                                var directory = Path.GetDirectoryName(
                                    Path.GetFullPath(options.Output)
                                )!;
                                var stem = Path.GetFileNameWithoutExtension(options.Output);
                                var extension = Path.GetExtension(options.Output);
                                var parts = Split(output, splitSize.Value);
                                for (var index = 0; index < parts.Count; index++)
                                    await File.WriteAllTextAsync(
                                        Path.Combine(
                                            directory,
                                            $"{stem}.part{index + 1:D3}{extension}"
                                        ),
                                        parts[index]
                                    );
                            }
                            var message = splitSize is null
                                ? $"Output saved to {options.Output}"
                                : $"Output split into {Split(output, splitSize.Value).Count:N0} parts beside {options.Output}";
                            SpectreExtensions.WriteSuccess(message);
                        }

                        // Copy to clipboard
                        if (
                            options.Copy
                            || (!options.Stdout && string.IsNullOrWhiteSpace(options.Output))
                        )
                        {
                            await clipboardService.CopyToClipboardAsync(output);
                            SpectreExtensions.WriteSuccess("Content copied to clipboard");
                        }

                        // Token count
                        if (options.TokenCount)
                        {
                            var tokenTree = tokenEstimator.EstimatePerFile(repositoryContext);

                            AnsiConsole.MarkupLine(
                                $"[grey]Estimated token count: {totalTokens:N0}[/]"
                            );

                            var tree = new Tree("📊 Token Count by File");
                            foreach (
                                var (file, tokens) in tokenTree
                                    .OrderByDescending(x => x.Value)
                                    .Take(10)
                            )
                            {
                                tree.AddNode($"[grey]{file}[/]: {tokens:N0} tokens");
                            }
                            AnsiConsole.Write(tree);
                        }

                        // Show summary
                        var destination =
                            options.Stdout ? "stdout"
                            : !string.IsNullOrWhiteSpace(options.Output) ? options.Output
                            : "clipboard";
                        if (options.Copy && destination != "clipboard")
                            destination += " + clipboard";

                        SpectreExtensions.WriteSummary(
                            ("Directory", originalDirectory),
                            ("Files processed", repositoryContext.FileCount.ToString("N0")),
                            ("Total size", $"{repositoryContext.TotalSize:N0} bytes"),
                            ("Security findings", findings.Count.ToString("N0")),
                            ("Output format", options.Style.ToLowerInvariant()),
                            ("Destination", destination)
                        );
                    }
                );
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error packing repository");
            AnsiConsole.WriteException(ex, ExceptionFormats.ShortenEverything);
            return 1;
        }
        finally
        {
            Console.SetOut(originalOutput);
            if (stdoutOutput is not null)
                Console.Write(stdoutOutput);
            if (temporaryDirectory is not null)
                Directory.Delete(temporaryDirectory, true);
            options.Directory = originalDirectory;
        }

        return Environment.ExitCode == 2 ? 2 : 0;
    }

    private static ISecurityScanner GetSecurityScanner(
        string? scanner,
        ISecurityScanner builtInScanner,
        SecretlintSecurityScanner secretlintScanner
    ) =>
        scanner?.Trim().ToLowerInvariant() switch
        {
            null or "" or "built-in" or "builtin" => builtInScanner,
            "secretlint" => secretlintScanner,
            _ => throw new InvalidOperationException(
                $"Unknown security scanner '{scanner}'. Use 'built-in' or 'secretlint'."
            ),
        };

    private static async Task<int> InitializeConfigAsync(string directory)
    {
        var path = Path.Combine(Path.GetFullPath(directory), "promptpack.config.json");
        if (File.Exists(path))
            throw new InvalidOperationException($"Configuration file already exists: {path}");

        await File.WriteAllTextAsync(
            path,
            "{\n  \"include\": null,\n  \"exclude\": null,\n  \"useGitignore\": true,\n  \"useDefaultPatterns\": true,\n  \"compress\": false,\n  \"removeComments\": false,\n  \"removeEmptyLines\": false,\n  \"securityScan\": true,\n  \"securityScanner\": \"built-in\"\n}\n"
        );
        AnsiConsole.MarkupLine($"[green]✓[/] Created configuration at [blue]{path}[/]");
        return 0;
    }

    private static async Task<string> CloneRemoteAsync(string remote, string? branch)
    {
        var directory = Directory.CreateTempSubdirectory("promptpack-remote-").FullName;
        var arguments =
            $"clone --depth 1 {(string.IsNullOrWhiteSpace(branch) ? "" : $"--branch {Quote(branch)} ")}{Quote(remote)} {Quote(directory)}";
        var startInfo = new ProcessStartInfo("git", arguments)
        {
            RedirectStandardError = true,
            RedirectStandardOutput = true,
            UseShellExecute = false,
        };
        using var process =
            Process.Start(startInfo) ?? throw new InvalidOperationException("Could not start git.");
        var error = await process.StandardError.ReadToEndAsync();
        await process.WaitForExitAsync();
        if (process.ExitCode != 0)
        {
            Directory.Delete(directory, true);
            throw new InvalidOperationException(
                $"Could not clone remote repository: {error.Trim()}"
            );
        }
        return directory;
    }

    private static string Quote(string value) => $"\"{value.Replace("\"", "\\\"")}\"";

    private static long? ParseSize(string? value)
    {
        if (string.IsNullOrWhiteSpace(value))
            return null;
        var normalized = value.Trim().ToUpperInvariant();
        var multiplier =
            normalized.EndsWith("MB") ? 1024L * 1024
            : normalized.EndsWith("KB") ? 1024L
            : 1;
        var number = normalized.TrimEnd('M', 'B', 'K');
        return long.TryParse(number, out var parsed) && parsed > 0
            ? parsed * multiplier
            : throw new ArgumentException("Split size must be a positive byte, KB, or MB value.");
    }

    private static List<string> Split(string output, long size)
    {
        var parts = new List<string>();
        var current = new StringBuilder();
        var currentBytes = 0L;
        foreach (var character in output)
        {
            var characterBytes = Encoding.UTF8.GetByteCount([character]);
            if (current.Length > 0 && currentBytes + characterBytes > size)
            {
                parts.Add(current.ToString());
                current.Clear();
                currentBytes = 0;
            }

            current.Append(character);
            currentBytes += characterBytes;
        }

        if (current.Length > 0)
            parts.Add(current.ToString());
        return parts;
    }
}
