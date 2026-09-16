using PromptPack.Services;

namespace PromptPack.Core.Tests.Unit;

public sealed class SecurityScannerTests
{
    [Fact]
    public async Task ScanAsync_FindsKnownCredentialPatterns()
    {
        var directory = Directory.CreateTempSubdirectory("promptpack-security-");
        try
        {
            var path = Path.Combine(directory.FullName, "secrets.txt");
            await File.WriteAllTextAsync(
                path,
                "private: -----BEGIN PRIVATE KEY-----\naws: AKIA1234567890ABCD\ngh: ghp_12345678901234567890",
                TestContext.Current.CancellationToken
            );

            var findings = await new SecurityScanner().ScanAsync([path], directory.FullName);

            findings.ShouldHaveSingleItem();
            findings[0].RelativePath.ShouldBe("secrets.txt");
            findings[0].Rule.ShouldBe("private key");
        }
        finally
        {
            directory.Delete(true);
        }
    }

    [Fact]
    public async Task ScanAsync_FindsGenericSecretAssignmentButIgnoresShortValues()
    {
        var directory = Directory.CreateTempSubdirectory("promptpack-security-");
        try
        {
            var path = Path.Combine(directory.FullName, "config.txt");
            await File.WriteAllTextAsync(
                path,
                "password = short\napi_key: long-enough-secret-value",
                TestContext.Current.CancellationToken
            );

            var findings = await new SecurityScanner().ScanAsync([path], directory.FullName);

            findings.ShouldHaveSingleItem();
            findings[0].Rule.ShouldBe("generic secret assignment");
        }
        finally
        {
            directory.Delete(true);
        }
    }
}
