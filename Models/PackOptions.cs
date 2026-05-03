namespace PromptPack.Models;

public record PackOptions
{
public string Directory { get; init; } = ".";
public string Output { get; init; } = "repomix-output.xml";
public string Style { get; init; } = "xml";
public bool Copy { get; init; }
public bool Stdout { get; init; }
public string? Include { get; init; }
public string? Ignore { get; init; }
public bool Compress { get; init; }
public bool RemoveComments { get; init; }
public bool TokenCount { get; init; }
public bool Verbose { get; init; }
}