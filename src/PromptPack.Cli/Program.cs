using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using PromptPack.Commands;
using PromptPack.Extensions;
using PromptPack.Services;
using PromptPack.Services.Interfaces;
using PromptPack.Services.OutputGenerators;
using Serilog;
using Spectre.Console.Cli;

var services = new ServiceCollection();

Log.Logger = new LoggerConfiguration().MinimumLevel.Information().WriteTo.Console().CreateLogger();

services.AddLogging(builder =>
{
    builder.ClearProviders();
    builder.AddSerilog();
});

services.AddSingleton<IFileCollector, FileCollector>();
services.AddSingleton<IConfigurationLoader, ConfigurationLoader>();
services.AddSingleton<ISecurityScanner, SecurityScanner>();
services.AddSingleton<IContentProcessor, ContentProcessor>();
services.AddSingleton<IClipboardService, ClipboardService>();
services.AddSingleton<TokenEstimator>();
services.AddSingleton<IOutputGenerator, XmlOutputGenerator>();
services.AddSingleton<IOutputGenerator, MarkdownOutputGenerator>();
services.AddSingleton<IOutputGenerator, JsonOutputGenerator>();
services.AddSingleton<IOutputGenerator, PlainTextOutputGenerator>();
services.AddSingleton<PackCommand>();

using var serviceProvider = services.BuildServiceProvider();
var logger = serviceProvider.GetRequiredService<ILogger<Program>>();

var registrar = new ServiceProviderTypeRegistrar(serviceProvider);

var app = new CommandApp(registrar);
app.SetDefaultCommand<PackCommand>();
app.Configure(config =>
{
    config.SetApplicationName("promptpack");
    config.SetApplicationVersion("1.0.0");
    config.Settings.ShowOptionDefaultValues = true;
    config.Settings.ExceptionHandler = exception =>
    {
        logger.LogError(exception, "Unexpected error occurred");
        SpectreExtensions.WriteError(exception.Message);
        return 1;
    };

    config
        .AddCommand<PackCommand>("pack")
        .WithDescription("Package a repository into AI-friendly context")
        .WithAlias("p");
});

try
{
    var writesToStdout = args.Contains("--stdout") || args.Contains("-S") || args.Contains("-");
    if (!writesToStdout)
        SpectreExtensions.WriteHeader();

    return await app.RunAsync(args);
}
catch (Exception exception)
{
    logger.LogCritical(exception, "Application failed to start");
    SpectreExtensions.WriteError($"Fatal error: {exception.Message}");
    return 1;
}
finally
{
    Log.CloseAndFlush();
}
