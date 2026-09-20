using PromptPack.Models;
using PromptPack.Services;

namespace PromptPack.Core.Tests.Unit;

public class TokenEstimatorTests
{
    [Fact]
    public void Should_Return_Zero_For_Empty_Content()
    {
        new TokenEstimator().EstimateTokens(string.Empty).ShouldBe(0);
    }

    [Fact]
    public void Should_Return_An_Estimate_For_Each_Relative_Path()
    {
        var context = new RepositoryContext
        {
            BaseDirectory = ".",
            Files =
            [
                ProcessedFile.Create("README.md", ".", "hello world"),
                ProcessedFile.Create("src/Program.cs", ".", "class Program {}"),
            ],
        };

        var estimates = new TokenEstimator().EstimatePerFile(context);

        estimates.Count.ShouldBe(2);
        estimates.Values.ShouldAllBe(estimate => estimate > 0);
        estimates.Keys.ShouldContain("README.md");
    }
}
