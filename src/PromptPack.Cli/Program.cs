using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using PromptPack.Cli.Input;
using PromptPack.Cli.Platform;
using PromptPack.Cli.Security;
using PromptPack.Commands;
using PromptPack.Extensions;
using PromptPack.Services;
using PromptPack.Services.Interfaces;
using PromptPack.Services.OutputGenerators;
using Spectre.Console.Cli;

var builder = Host.CreateApplicationBuilder(args);
var verboseLogging = args.Contains("--verbose") || args.Contains("-w");

builder.Logging.ClearProviders();
builder.Logging.AddSimpleConsole(options =>
{
    options.SingleLine = true;
    options.TimestampFormat = "HH:mm:ss ";
});
builder.Logging.SetMinimumLevel(verboseLogging ? LogLevel.Debug : LogLevel.Information);

builder.Services.AddSingleton<IFileCollector, FileCollector>();
builder.Services.AddSingleton<IConfigurationLoader, ConfigurationLoader>();
builder.Services.AddSingleton<ISecurityScanner, SecurityScanner>();
builder.Services.AddSingleton<ISecretlintProcessRunner, SecretlintProcessRunner>();
builder.Services.AddSingleton<SecretlintSecurityScanner>();
builder.Services.AddSingleton<IContentProcessor, ContentProcessor>();
builder.Services.AddSingleton<IClipboardService, ClipboardService>();
builder.Services.AddSingleton<StdinPathReader>();
builder.Services.AddSingleton<TokenEstimator>();
builder.Services.AddSingleton<IOutputGenerator, XmlOutputGenerator>();
builder.Services.AddSingleton<IOutputGenerator, MarkdownOutputGenerator>();
builder.Services.AddSingleton<IOutputGenerator, JsonOutputGenerator>();
builder.Services.AddSingleton<IOutputGenerator, PlainTextOutputGenerator>();
builder.Services.AddSingleton<PackCommand>();

using var host = builder.Build();
var logger = host.Services.GetRequiredService<ILogger<Program>>();

var registrar = new ServiceProviderTypeRegistrar(host.Services);

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
});

try
{
    var writesToStdout = args.Contains("--stdout") || args.Contains("-s") || args.Contains("-");
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
