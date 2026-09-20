using PromptPack.Models;

namespace PromptPack.Core.Tests.Unit;

public class RepositoryContextTests
{
    [Fact]
    public void Should_Return_Sorted_Indented_Directory_Tree()
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

        structure.ShouldBe(expectedStructure);
        context.FileCount.ShouldBe(3);
        context.TotalSize.ShouldBe(8);
    }
}
