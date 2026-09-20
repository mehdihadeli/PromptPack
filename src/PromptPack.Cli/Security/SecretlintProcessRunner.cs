using System.Diagnostics;

namespace PromptPack.Cli.Security;

public sealed class SecretlintProcessRunner : ISecretlintProcessRunner
{
    public async Task<SecretlintProcessResult> RunAsync(
        IReadOnlyList<string> files,
        string baseDirectory,
        CancellationToken cancellationToken = default
    )
    {
        var commands = OperatingSystem.IsWindows() ? new[] { "npx.cmd", "npx" } : new[] { "npx" };

        foreach (var command in commands)
        {
            var startInfo = new ProcessStartInfo
            {
                FileName = command,
                WorkingDirectory = baseDirectory,
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                UseShellExecute = false,
                CreateNoWindow = true,
            };

            startInfo.ArgumentList.Add("@secretlint/quick-start");
            startInfo.ArgumentList.Add("--format=json");
            startInfo.ArgumentList.Add("--no-color");
            startInfo.ArgumentList.Add("--no-terminalLink");
            startInfo.ArgumentList.Add("--no-glob");
            foreach (var file in files)
                startInfo.ArgumentList.Add(file);

            try
            {
                using var process =
                    Process.Start(startInfo)
                    ?? throw new InvalidOperationException($"Unable to start {command}.");
                var standardOutput = await process.StandardOutput.ReadToEndAsync(cancellationToken);
                var standardError = await process.StandardError.ReadToEndAsync(cancellationToken);
                await process.WaitForExitAsync(cancellationToken);
                return new SecretlintProcessResult(process.ExitCode, standardOutput, standardError);
            }
            catch (System.ComponentModel.Win32Exception)
            {
                // Try the next executable name on Windows.
            }
        }

        throw new InvalidOperationException(
            "Secretlint was selected but Node.js/npm was not found. "
                + "Install Node.js, then verify that 'npx @secretlint/quick-start \"**/*\"' runs."
        );
    }
}
