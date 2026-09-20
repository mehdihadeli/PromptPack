using PromptPack.Models;
using PromptPack.Services.OutputGenerators;

namespace PromptPack.Core.Tests.Unit;

public class MarkdownOutputGeneratorTests
{
    [Fact]
    public void Should_Add_Line_Numbers_And_Repository_Sections()
    {
        var context = new RepositoryContext
        {
            BaseDirectory = ".",
            ShowLineNumbers = true,
            Files = [ProcessedFile.Create("src/Program.cs", ".", "class Program\n{\n}\n")],
        };

        var output = new MarkdownOutputGenerator().Generate(context);

        output.ShouldContain("# Repository Context for AI Analysis");
        output.ShouldContain("## 📁 Repository Structure");
        output.ShouldContain("### 📝 src/Program.cs");
        output.ShouldContain("     1 | class Program");
    }
}
