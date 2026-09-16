using PromptPack.Models;
using PromptPack.Services;

namespace PromptPack.Core.Tests.Unit;

public class TokenEstimatorTests
{
    [Fact]
    public void EstimateTokens_EmptyContentReturnsZero()
    {
        new TokenEstimator().EstimateTokens(string.Empty).ShouldBe(0);
    }

    [Fact]
    public void EstimatePerFile_ReturnsEstimateForEachRelativePath()
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
