using PromptPack.Cli.Security;

namespace PromptPack.Cli.Tests.Unit;

public sealed class SecretlintSecurityScannerTests
{
    [Fact]
    public async Task Should_Map_Secretlint_Messages_To_Relative_Findings()
    {
        var baseDirectory = Directory.CreateTempSubdirectory("promptpack-secretlint-");
        try
        {
            var file = Path.Combine(baseDirectory.FullName, "config", "secrets.env");
            var runner = new FakeSecretlintProcessRunner(
                new SecretlintProcessResult(
                    1,
                    $$"""
                    [{
                      "filePath": "{{file.Replace("\\", "/")}}",
                      "messages": [
                        { "ruleId": "@secretlint/secretlint-rule-aws", "message": "masked" },
                        { "messageId": "PRIVATE_KEY", "message": "masked" }
                      ]
                    }]
                    """,
                    string.Empty
                )
            );
            var scanner = new SecretlintSecurityScanner(runner);

            var findings = await scanner.ScanAsync([file], baseDirectory.FullName);

            findings.Count.ShouldBe(2);
            findings[0].RelativePath.ShouldBe("config/secrets.env");
            findings[0].Rule.ShouldBe("@secretlint/secretlint-rule-aws");
            findings[1].Rule.ShouldBe("PRIVATE_KEY");
            runner.Files.ShouldBe([file]);
            runner.BaseDirectory.ShouldBe(baseDirectory.FullName);
        }
        finally
        {
            baseDirectory.Delete(true);
        }
    }

    [Fact]
    public async Task Should_Parse_Nested_Results_And_Use_Fallback_Rule_Name()
    {
        var baseDirectory = Directory.CreateTempSubdirectory("promptpack-secretlint-");
        try
        {
            var file = Path.Combine(baseDirectory.FullName, "secrets.txt");
            var runner = new FakeSecretlintProcessRunner(
                new SecretlintProcessResult(
                    1,
                    $$"""
                    {
                      "results": [{
                        "file": "{{file.Replace("\\", "/")}}",
                        "messages": [{ "message": "masked" }]
                      }]
                    }
                    """,
                    string.Empty
                )
            );

            var findings = await new SecretlintSecurityScanner(runner).ScanAsync(
                [file],
                baseDirectory.FullName
            );

            findings.ShouldHaveSingleItem();
            findings[0].RelativePath.ShouldBe("secrets.txt");
            findings[0].Rule.ShouldBe("Secretlint finding");
        }
        finally
        {
            baseDirectory.Delete(true);
        }
    }

    [Fact]
    public async Task Should_Return_No_Findings_When_Secretlint_Produces_No_Output()
    {
        var runner = new FakeSecretlintProcessRunner(new SecretlintProcessResult(0, "", ""));

        var findings = await new SecretlintSecurityScanner(runner).ScanAsync([], ".");

        findings.ShouldBeEmpty();
    }

    [Fact]
    public async Task Should_Allow_Exit_Code_One_When_Findings_Exist()
    {
        var runner = new FakeSecretlintProcessRunner(
            new SecretlintProcessResult(1, "[{\"filePath\":\"secrets.txt\",\"messages\":[]}]", "")
        );

        var findings = await new SecretlintSecurityScanner(runner).ScanAsync([], ".");

        findings.ShouldBeEmpty();
    }

    [Fact]
    public async Task Should_Reject_Unexpected_Exit_Codes()
    {
        var runner = new FakeSecretlintProcessRunner(
            new SecretlintProcessResult(2, "", "Node.js failed")
        );

        var exception = await Should.ThrowAsync<InvalidOperationException>(
            () => new SecretlintSecurityScanner(runner).ScanAsync([], ".")
        );

        exception.Message.ShouldContain("exit code 2");
        exception.Message.ShouldContain("Node.js failed");
    }

    [Fact]
    public async Task Should_Reject_Invalid_Json()
    {
        var runner = new FakeSecretlintProcessRunner(
            new SecretlintProcessResult(0, "not json", "")
        );

        var exception = await Should.ThrowAsync<InvalidOperationException>(
            () => new SecretlintSecurityScanner(runner).ScanAsync([], ".")
        );

        exception.Message.ShouldContain("invalid JSON");
    }

    private sealed class FakeSecretlintProcessRunner(SecretlintProcessResult result)
        : ISecretlintProcessRunner
    {
        public IReadOnlyList<string> Files { get; private set; } = [];
        public string BaseDirectory { get; private set; } = string.Empty;

        public Task<SecretlintProcessResult> RunAsync(
            IReadOnlyList<string> files,
            string baseDirectory,
            CancellationToken cancellationToken = default
        )
        {
            Files = files;
            BaseDirectory = baseDirectory;
            return Task.FromResult(result);
        }
    }
}
