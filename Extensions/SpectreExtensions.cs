using Spectre.Console;

namespace PromptPack.Extensions;

public static class SpectreExtensions
{
    public static void WriteHeader()
    {
        AnsiConsole.Write(new FigletText("RepoPacker")
            .Centered()
            .Color(Color.Blue));
        
        var rule = new Rule("[grey]AI-Friendly Repository Context Packer[/]");
        rule.Style = Style.Parse("blue dim");
        AnsiConsole.Write(rule);
        AnsiConsole.WriteLine();
    }
    
    public static void WriteError(string message)
    {
        AnsiConsole.MarkupLine($"[red bold]Error:[/] {message}");
    }
    
    public static void WriteWarning(string message)
    {
        AnsiConsole.MarkupLine($"[yellow]Warning:[/] {message}");
    }
    
    public static void WriteSuccess(string message)
    {
        AnsiConsole.MarkupLine($"[green]✓[/] {message}");
    }
    
    public static void WriteInfo(string message)
    {
        AnsiConsole.MarkupLine($"[grey]ℹ[/] {message}");
    }
    
    public static async Task RunWithSpinner(this Task task, string message, Action action)
    {
        await AnsiConsole.Status()
            .StartAsync(message, async ctx =>
            {
                action();
                await task;
            });
    }
}