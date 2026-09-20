using Spectre.Console;

namespace PromptPack.Extensions;

public static class SpectreExtensions
{
    public static void WriteHeader()
    {
        AnsiConsole.Write(new FigletText("PromptPack").Centered().Color(Color.Blue));

        var rule = new Rule("[grey]AI-Friendly Repository Context Packer[/]");
        rule.Style = Style.Parse("blue dim");
        AnsiConsole.Write(rule);
        AnsiConsole.WriteLine();
    }

    public static void WriteError(string message)
    {
        AnsiConsole.MarkupLine($"[red bold]Error:[/] {Markup.Escape(message)}");
    }

    public static void WriteWarning(string message)
    {
        AnsiConsole.MarkupLine($"[yellow]Warning:[/] {Markup.Escape(message)}");
    }

    public static void WriteSuccess(string message)
    {
        AnsiConsole.MarkupLine($"[green]✓[/] {Markup.Escape(message)}");
    }

    public static void WriteInfo(string message)
    {
        AnsiConsole.MarkupLine($"[grey]ℹ[/] {Markup.Escape(message)}");
    }

    public static void WriteStage(string title, string detail)
    {
        AnsiConsole.MarkupLine(
            $"[deepskyblue1]›[/] [bold]{Markup.Escape(title)}[/] [grey]{Markup.Escape(detail)}[/]"
        );
    }

    public static void WriteSummary(params (string Label, string Value)[] rows)
    {
        var table = new Table { Border = TableBorder.Rounded };
        table.AddColumn(new TableColumn("[grey]Metric[/]"));
        table.AddColumn(new TableColumn("[grey]Value[/]"));

        foreach (var (label, value) in rows)
            table.AddRow(Markup.Escape(label), Markup.Escape(value));

        AnsiConsole.Write(
            new Panel(table)
            {
                Header = new PanelHeader("📦 Pack Complete"),
                Border = BoxBorder.Rounded,
                Padding = new Padding(1, 1),
            }
        );
    }

    public static async Task RunWithSpinner(this Task task, string message, Action action)
    {
        await AnsiConsole
            .Status()
            .StartAsync(
                message,
                async ctx =>
                {
                    action();
                    await task;
                }
            );
    }
}
