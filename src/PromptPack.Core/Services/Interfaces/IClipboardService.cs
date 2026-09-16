namespace PromptPack.Services.Interfaces;

public interface IClipboardService
{
    Task CopyToClipboardAsync(string content);
}