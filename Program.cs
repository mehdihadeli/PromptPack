// src/RepoPacker/Program.cs
using System.CommandLine;
using System.CommandLine.Builder;
using System.CommandLine.Parsing;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using PromptPack.Commands;
using PromptPack.Extensions;
using PromptPack.Services;
using PromptPack.Services.Interfaces;
using PromptPack.Services.OutputGenerators;
using Serilog;

var services = new ServiceCollection();

// Configure logging
Log.Logger = new LoggerConfiguration()
    .MinimumLevel.Information()
    .WriteTo.Console()
    .CreateLogger();

services.AddLogging(builder =>
{
    builder.ClearProviders();
    builder.AddSerilog();
});

// Register services
services.AddSingleton<IFileCollector, FileCollector>();
services.AddSingleton<IContentProcessor, ContentProcessor>();
services.AddSingleton<IClipboardService, ClipboardService>();
services.AddSingleton<TokenEstimator>();

// Register output generators
services.AddSingleton<IOutputGenerator, XmlOutputGenerator>();
services.AddSingleton<IOutputGenerator, MarkdownOutputGenerator>();
services.AddSingleton<IOutputGenerator, JsonOutputGenerator>();
services.AddSingleton<IOutputGenerator, PlainTextOutputGenerator>();

services.AddSingleton<PackCommand>();

var serviceProvider = services.BuildServiceProvider();
var logger = serviceProvider.GetRequiredService<ILogger<Program>>();

try
{
    SpectreExtensions.WriteHeader();
    
    var packCommand = serviceProvider.GetRequiredService<PackCommand>();
    
    // Create root command with options
    var rootCommand = new RootCommand("AI-friendly repository context packer");
    
    // Output options
    var outputOption = new Option<string>(
        ["--output", "-o"], 
        () => "repomix-output.xml",
        "Output file path (use '-' for stdout)");
    
    var styleOption = new Option<string>(
        "--style",
        () => "xml",
        "Output format: xml, markdown, json, or plain");
    
    var copyOption = new Option<bool>(
        "--copy",
        () => false,
        "Copy output to clipboard");
    
    var stdoutOption = new Option<bool>(
        "--stdout",
        () => false,
        "Write output to stdout instead of file");
    
    // File selection options
    var includeOption = new Option<string?>(
        "--include",
        "Include only files matching these glob patterns (comma-separated)");
    
    var ignoreOption = new Option<string?>(
        ["--ignore", "-i"],
        "Additional patterns to exclude (comma-separated)");
    
    // Processing options
    var compressOption = new Option<bool>(
        "--compress",
        () => false,
        "Extract essential code structure");
    
    var removeCommentsOption = new Option<bool>(
        "--remove-comments",
        () => false,
        "Strip code comments");
    
    var tokenCountOption = new Option<bool>(
        "--token-count",
        () => false,
        "Show token count estimates");
    
    var verboseOption = new Option<bool>(
        ["--verbose", "-v"],
        () => false,
        "Enable verbose output");
    
    // Add all options
    rootCommand.AddOption(outputOption);
    rootCommand.AddOption(styleOption);
    rootCommand.AddOption(copyOption);
    rootCommand.AddOption(stdoutOption);
    rootCommand.AddOption(includeOption);
    rootCommand.AddOption(ignoreOption);
    rootCommand.AddOption(compressOption);
    rootCommand.AddOption(removeCommentsOption);
    rootCommand.AddOption(tokenCountOption);
    rootCommand.AddOption(verboseOption);
    
    // Add directory argument
    var directoryArgument = new Argument<string>(
        "directory",
        () => ".",
        "Directory to pack");
    rootCommand.AddArgument(directoryArgument);
    
    rootCommand.SetHandler(async (context) =>
    {
        await packCommand.ExecuteAsync(rootCommand, context);
    });
    
    // Build parser with middleware
    var parser = new CommandLineBuilder(rootCommand)
        .UseDefaults()
        .UseExceptionHandler((ex, context) =>
        {
            logger.LogError(ex, "Unexpected error occurred");
            SpectreExtensions.WriteError(ex.Message);
            context.ExitCode = 1;
        })
        .Build();
    
    return await parser.InvokeAsync(args);
}
catch (Exception ex)
{
    logger.LogCritical(ex, "Application failed to start");
    SpectreExtensions.WriteError($"Fatal error: {ex.Message}");
    return 1;
}
finally
{
    Log.CloseAndFlush();
}