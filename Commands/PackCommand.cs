using System.CommandLine;
using System.CommandLine.Invocation;
using Microsoft.Extensions.Logging;
using PromptPack.Models;
using PromptPack.Services;
using PromptPack.Services.Interfaces;
using Spectre.Console;

namespace PromptPack.Commands;

public class PackCommand(
    IFileCollector fileCollector,
    IContentProcessor contentProcessor,
    IEnumerable<IOutputGenerator> outputGenerators,
    IClipboardService clipboardService,
    TokenEstimator tokenEstimator,
    ILogger<PackCommand> logger)
{
    public async Task ExecuteAsync(RootCommand rootCommand, InvocationContext context)
    {
        var options = ParseOptions(context);
        
        try
        {
            await AnsiConsole.Status()
                .StartAsync("Packing repository...", async ctx =>
                {
                    ctx.Status("Collecting files...");
                    
                    // Collect files
                    var files = await fileCollector.CollectFilesAsync(
                        options.Directory, options.Include, options.Ignore);
                    
                    if (files.Count == 0)
                    {
                        AnsiConsole.MarkupLine("[yellow]No files found matching the criteria.[/]");
                        return;
                    }
                    
                    // Process files
                    ctx.Status("Processing files...");
                    var processedFiles = new List<ProcessedFile>();
                    
                    await AnsiConsole.Progress()
                        .AutoClear(false)
                        .Columns(new TaskDescriptionColumn(), new ProgressBarColumn(), new PercentageColumn())
                        .StartAsync(async progressContext =>
                        {
                            var task = progressContext.AddTask("[green]Processing[/]", maxValue: files.Count);
                            
                            foreach (var file in files)
                            {
                                var processedFile = await contentProcessor.ProcessFileAsync(
                                    file, options.Directory, options.RemoveComments, options.Compress);
                                processedFiles.Add(processedFile);
                                task.Increment(1);
                            }
                        });
                    
                    // Create repository context
                    var repositoryContext = new RepositoryContext
                    {
                        BaseDirectory = options.Directory,
                        Files = processedFiles,
                        GeneratedAt = DateTime.UtcNow
                    };
                    
                    // Generate output
                    ctx.Status("Generating output...");
                    var generator = outputGenerators.First(g => g.Style == options.Style.ToLower());
                    var output = generator.Generate(repositoryContext);
                    
                    // Handle output destinations
                    if (options.Stdout || options.Output == "-")
                    {
                        AnsiConsole.WriteLine(output);
                    }
                    else
                    {
                        await File.WriteAllTextAsync(options.Output, output);
                        AnsiConsole.MarkupLine($"[green]✓[/] Output saved to [blue]{options.Output}[/]");
                    }
                    
                    // Copy to clipboard
                    if (options.Copy)
                    {
                        await clipboardService.CopyToClipboardAsync(output);
                        AnsiConsole.MarkupLine("[green]✓[/] Content copied to clipboard");
                    }
                    
                    // Token count
                    if (options.TokenCount)
                    {
                        var totalTokens = tokenEstimator.EstimateTokens(output);
                        var tokenTree = tokenEstimator.EstimatePerFile(repositoryContext);
                        
                        AnsiConsole.MarkupLine($"[grey]Estimated token count: {totalTokens:N0}[/]");
                        
                        var tree = new Tree("📊 Token Count by File");
                        foreach (var (file, tokens) in tokenTree.OrderByDescending(x => x.Value).Take(10))
                        {
                            tree.AddNode($"[grey]{file}[/]: {tokens:N0} tokens");
                        }
                        AnsiConsole.Write(tree);
                    }
                    
                    // Show summary
                    var panel = new Panel(
                        $"[bold]Repository Summary[/]\n" +
                        $"[grey]Directory:[/] {options.Directory}\n" +
                        $"[grey]Files processed:[/] {repositoryContext.FileCount}\n" +
                        $"[grey]Total size:[/] {repositoryContext.TotalSize:N0} bytes\n" +
                        $"[grey]Output format:[/] {options.Style}")
                    {
                        Header = new PanelHeader("📦 Pack Complete"),
                        Border = BoxBorder.Rounded,
                        Padding = new Padding(2, 1)
                    };
                    AnsiConsole.Write(panel);
                });
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error packing repository");
            AnsiConsole.WriteException(ex, ExceptionFormats.ShortenEverything);
        }
    }
    
    private static PackOptions ParseOptions(InvocationContext context)
    {
        return new PackOptions
        {
            Directory = context.ParseResult.GetValueForArgument(
                (Argument<string>)context.ParseResult.CommandResult.Command.Arguments[0]),
            Output = context.ParseResult.GetValueForOption(
                (Option<string>)context.ParseResult.CommandResult.Command.Options[0]) ?? "repomix-output.xml",
            Style = context.ParseResult.GetValueForOption(
                (Option<string>)context.ParseResult.CommandResult.Command.Options[1]) ?? "xml",
            Copy = context.ParseResult.GetValueForOption(
                (Option<bool>)context.ParseResult.CommandResult.Command.Options[2]),
            Stdout = context.ParseResult.GetValueForOption(
                (Option<bool>)context.ParseResult.CommandResult.Command.Options[3]),
            Include = context.ParseResult.GetValueForOption(
                (Option<string>?)context.ParseResult.CommandResult.Command.Options.ElementAtOrDefault(4)),
            Ignore = context.ParseResult.GetValueForOption(
                (Option<string>?)context.ParseResult.CommandResult.Command.Options.ElementAtOrDefault(5)),
            Compress = context.ParseResult.GetValueForOption(
                (Option<bool>)context.ParseResult.CommandResult.Command.Options[6]),
            RemoveComments = context.ParseResult.GetValueForOption(
                (Option<bool>)context.ParseResult.CommandResult.Command.Options[7]),
            TokenCount = context.ParseResult.GetValueForOption(
                (Option<bool>)context.ParseResult.CommandResult.Command.Options[8])
        };
    }
}