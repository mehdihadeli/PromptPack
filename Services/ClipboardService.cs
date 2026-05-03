using System.Diagnostics;
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
            if (OperatingSystem.IsWindows())
            {
                await CopyWindowsAsync(content);
            }
            else if (OperatingSystem.IsMacOS())
            {
                await CopyMacOSAsync(content);
            }
            else if (OperatingSystem.IsLinux())
            {
                await CopyLinuxAsync(content);
            }
            else
            {
                throw new PlatformNotSupportedException("Clipboard not supported on this OS");
            }
            
            logger.LogInformation("Content copied to clipboard successfully");
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Failed to copy to clipboard");
            throw;
        }
    }
    
    private static async Task CopyWindowsAsync(string content)
    {
        using var process = new Process
        {
            StartInfo = new ProcessStartInfo
            {
                FileName = "clip",
                RedirectStandardInput = true,
                UseShellExecute = false,
                CreateNoWindow = true
            }
        };
        
        process.Start();
        await process.StandardInput.WriteAsync(content);
        process.StandardInput.Close();
        await process.WaitForExitAsync();
    }
    
    private static async Task CopyMacOSAsync(string content)
    {
        using var process = new Process
        {
            StartInfo = new ProcessStartInfo
            {
                FileName = "pbcopy",
                RedirectStandardInput = true,
                UseShellExecute = false,
                CreateNoWindow = true
            }
        };
        
        process.Start();
        await process.StandardInput.WriteAsync(content);
        process.StandardInput.Close();
        await process.WaitForExitAsync();
    }
    
    private static async Task CopyLinuxAsync(string content)
    {
        // Try xclip first, then wl-copy
        var copyCommands = new[] { "xclip", "wl-copy" };
        
        foreach (var command in copyCommands)
        {
            try
            {
                using var process = new Process
                {
                    StartInfo = new ProcessStartInfo
                    {
                        FileName = command,
                        RedirectStandardInput = true,
                        UseShellExecute = false,
                        CreateNoWindow = true
                    }
                };
                
                process.Start();
                await process.StandardInput.WriteAsync(content);
                process.StandardInput.Close();
                await process.WaitForExitAsync();
                return;
            }
            catch
            {
                // Try next command
            }
        }
        
        throw new InvalidOperationException("No clipboard utility found. Install xclip or wl-clipboard.");
    }
}