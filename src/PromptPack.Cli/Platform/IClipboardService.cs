namespace PromptPack.Cli.Platform;

public interface IClipboardService
{
    Task CopyToClipboardAsync(string content);
}
