namespace PromptPack.Cli.Security;

public interface ISecretlintProcessRunner
{
    Task<SecretlintProcessResult> RunAsync(
        IReadOnlyList<string> files,
        string baseDirectory,
        CancellationToken cancellationToken = default
    );
}

public sealed record SecretlintProcessResult(
    int ExitCode,
    string StandardOutput,
    string StandardError
);
