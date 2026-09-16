using PromptPack.Models;
using PromptPack.Services.OutputGenerators;

namespace PromptPack.Core.Tests.Unit;

public class MarkdownOutputGeneratorTests
{
    [Fact]
    public void Generate_AddsLineNumbersAndRepositorySections()
    {
        var context = new RepositoryContext
        {
            BaseDirectory = ".",
            ShowLineNumbers = true,
            Files =
            [
                ProcessedFile.Create(
                    "src/Program.cs",
                    ".",
                    "class Program\n{\n}\n"
                ),
            ],
        };

        var output = new MarkdownOutputGenerator().Generate(context);

        Assert.Contains("# Repository Context for AI Analysis", output);
        Assert.Contains("## 📁 Repository Structure", output);
        Assert.Contains("### 📝 src/Program.cs", output);
        Assert.Contains("     1 | class Program", output);
    }
}