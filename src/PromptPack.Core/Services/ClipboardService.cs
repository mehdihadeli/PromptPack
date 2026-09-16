using Microsoft.Extensions.Logging;
using PromptPack.Services.Interfaces;

namespace PromptPack.Services;

public class ClipboardService(ILogger<ClipboardService> logger) : IClipboardService
{
    public async Task CopyToClipboardAsync(string content)
    {
        logger.LogInformation("Copying to clipboard...");

        try
        {
            await TextCopy.ClipboardService.SetTextAsync(content);

            logger.LogInformation("Content copied to clipboard successfully");
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Failed to copy to clipboard");
            throw;
        }
    }
}
