namespace PromptPack.Models;

public sealed class PromptPackConfig
{
    public string? Include { get; set; }
    public string? Exclude { get; set; }
    public bool? UseGitignore { get; set; }
    public bool? UseDefaultPatterns { get; set; }
    public bool? Compress { get; set; }
    public bool? RemoveComments { get; set; }
    public bool? RemoveEmptyLines { get; set; }
    public bool? IncludeFullDirectoryStructure { get; set; }
    public bool? SecurityScan { get; set; }
    public string? SecurityScanner { get; set; }
    public string? HeaderText { get; set; }
    public string? InstructionFilePath { get; set; }
    public string? SplitOutput { get; set; }
}
