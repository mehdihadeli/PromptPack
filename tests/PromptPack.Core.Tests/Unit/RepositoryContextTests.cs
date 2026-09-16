using PromptPack.Models;

namespace PromptPack.Core.Tests.Unit;

public class RepositoryContextTests
{
    [Fact]
    public void GetDirectoryStructure_ReturnsSortedIndentedTree()
    {
        var context = new RepositoryContext
        {
            Files =
            [
                ProcessedFile.Create("src/Z.cs", ".", "z"),
                ProcessedFile.Create("README.md", ".", "readme"),
                ProcessedFile.Create("src/Models/A.cs", ".", "a"),
            ],
        };

        var structure = context.GetDirectoryStructure();

        var expectedStructure = string.Join(
            Environment.NewLine,
            ["README.md", "src/", "  Models/", "    A.cs", "  Z.cs", ""]
        );

        Assert.Equal(expectedStructure, structure);
        Assert.Equal(3, context.FileCount);
        Assert.Equal(8, context.TotalSize);
    }
}
