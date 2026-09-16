using System.Text;
using Microsoft.Extensions.Logging;
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
    TokenEstimator tokenEstimator,
    IConfigurationLoader configurationLoader,
    ISecurityScanner securityScanner,
    ILogger<PackCommand> logger
) : AsyncCommand<PackSettings>
{
    public override async Task<int> ExecuteAsync(CommandContext _, PackSettings options)
    {
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
                        var config = await configurationLoader.LoadAsync(
                            options.Directory,
                            options.Config
                        );
                        options.Include ??= config.Include;
                        options.Ignore ??= config.Exclude;
                        options.HeaderText ??= config.HeaderText;
                        options.InstructionFilePath ??= config.InstructionFilePath;
                        options.SplitOutput ??= config.SplitOutput;
                        options.Compress |= config.Compress ?? false;
                        options.RemoveComments |= config.RemoveComments ?? false;
                        options.RemoveEmptyLines |= config.RemoveEmptyLines ?? false;
                        options.SecurityScan |= config.SecurityScan ?? false;
                        options.IncludeFullDirectoryStructure |=
                            config.IncludeFullDirectoryStructure ?? false;

                        ctx.Status("Collecting files...");

                        // Collect files
                        var files = options.Stdin
                            ? await fileCollector.CollectPathsFromStdinAsync(
                                options.Directory,
                                options.Include,
                                options.Ignore
                            )
                            : await fileCollector.CollectFilesAsync(
                                options.Directory,
                                options.Include,
                                options.Ignore
                            );

                        if (files.Count == 0)
                        {
                            AnsiConsole.MarkupLine(
                                "[yellow]No files found matching the criteria.[/]"
                            );
                            return;
                        }

                        // Process files
                        ctx.Status("Processing files...");
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

                        var findings = options.SecurityScan
                            ? await securityScanner.ScanAsync(files, options.Directory)
                            : Array.Empty<SecurityFinding>();
                        var instructions = options.InstructionFilePath is null
                            ? null
                            : await File.ReadAllTextAsync(
                                Path.GetFullPath(options.InstructionFilePath, options.Directory)
                            );
                        var fullTree = options.IncludeFullDirectoryStructure
                            ? await fileCollector.CollectDirectoryPathsAsync(
                                options.Directory,
                                options.Ignore
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
                        ctx.Status("Generating output...");
                        var generator = outputGenerators.First(g =>
                            g.Style == options.Style.ToLower()
                        );
                        var output = generator.Generate(repositoryContext);

                        var totalTokens = tokenEstimator.EstimateTokens(output);
                        if (options.TokenBudget is { } tokenBudget && totalTokens > tokenBudget)
                        {
                            Console.Error.WriteLine(
                                $"Token budget exceeded: {totalTokens:N0} > {tokenBudget:N0}"
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
                            AnsiConsole.MarkupLine(
                                $"[green]✓[/] Output saved to [blue]{options.Output}[/]"
                            );
                        }

                        // Copy to clipboard
                        if (
                            options.Copy
                            || (!options.Stdout && string.IsNullOrWhiteSpace(options.Output))
                        )
                        {
                            await clipboardService.CopyToClipboardAsync(output);
                            AnsiConsole.MarkupLine("[green]✓[/] Content copied to clipboard");
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
                        var panel = new Panel(
                            $"[bold]Repository Summary[/]\n"
                                + $"[grey]Directory:[/] {options.Directory}\n"
                                + $"[grey]Files processed:[/] {repositoryContext.FileCount}\n"
                                + $"[grey]Total size:[/] {repositoryContext.TotalSize:N0} bytes\n"
                                + $"[grey]Output format:[/] {options.Style}"
                        )
                        {
                            Header = new PanelHeader("📦 Pack Complete"),
                            Border = BoxBorder.Rounded,
                            Padding = new Padding(2, 1),
                        };
                        AnsiConsole.Write(panel);
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
        }

        return Environment.ExitCode == 2 ? 2 : 0;
    }

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
