using Spectre.Console.Cli;

namespace PromptPack.Commands;

public sealed class PackSettings : CommandSettings
{
    [CommandArgument(0, "[DIRECTORY]")]
    public string Directory { get; set; } = ".";

    [CommandOption("-o|--output")]
    public string? Output { get; set; }

    [CommandOption("-y|--style")]
    public string Style { get; set; } = "markdown";

    [CommandOption("-c|--copy")]
    public bool Copy { get; set; }

    [CommandOption("-s|--stdout")]
    public bool Stdout { get; set; }

    [CommandOption("-i|--include")]
    public string? Include { get; set; }

    [CommandOption("-e|--exclude|--ignore")]
    public string? Ignore { get; set; }

    [CommandOption("-x|--compress")]
    public bool Compress { get; set; }

    [CommandOption("-r|--remove-comments")]
    public bool RemoveComments { get; set; }

    [CommandOption("-l|--remove-empty-lines")]
    public bool RemoveEmptyLines { get; set; }

    [CommandOption("-n|--output-show-line-numbers")]
    public bool ShowLineNumbers { get; set; }

    [CommandOption("-m|--no-file-summary")]
    public bool NoFileSummary { get; set; }

    [CommandOption("-d|--no-directory-structure")]
    public bool NoDirectoryStructure { get; set; }

    [CommandOption("-f|--no-files")]
    public bool NoFiles { get; set; }

    [CommandOption("-b|--token-budget")]
    public int? TokenBudget { get; set; }

    [CommandOption("-t|--token-count")]
    public bool TokenCount { get; set; }

    [CommandOption("-v|--verbose")]
    public bool Verbose { get; set; }

    [CommandOption("-g|--config")]
    public string? Config { get; set; }

    [CommandOption("-q|--stdin")]
    public bool Stdin { get; set; }

    [CommandOption("-k|--security-scan")]
    public bool SecurityScan { get; set; }

    [CommandOption("-h|--header-text")]
    public string? HeaderText { get; set; }

    [CommandOption("-p|--instruction-file-path")]
    public string? InstructionFilePath { get; set; }

    [CommandOption("-a|--include-full-directory-structure")]
    public bool IncludeFullDirectoryStructure { get; set; }

    [CommandOption("-z|--split-output")]
    public string? SplitOutput { get; set; }
}
