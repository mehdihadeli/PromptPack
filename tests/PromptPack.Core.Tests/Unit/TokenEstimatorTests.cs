using PromptPack.Models;
using PromptPack.Services;

namespace PromptPack.Core.Tests.Unit;

public class TokenEstimatorTests
{
    [Fact]
    public void EstimateTokens_EmptyContentReturnsZero()
    {
        Assert.Equal(0, new TokenEstimator().EstimateTokens(string.Empty));
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

        Assert.Equal(2, estimates.Count);
        Assert.All(estimates.Values, estimate => Assert.True(estimate > 0));
        Assert.Contains("README.md", estimates.Keys);
    }
}
