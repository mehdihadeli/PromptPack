using PromptPack.Models;
using PromptPack.Services.OutputGenerators;

namespace PromptPack.Core.Tests.Unit;

public sealed class FeatureOutputTests
{
    public static IEnumerable<object[]> Generators =>
        [
            [new MarkdownOutputGenerator()],
            [new JsonOutputGenerator()],
            [new XmlOutputGenerator()],
            [new PlainTextOutputGenerator()],
        ];

    [Theory]
    [MemberData(nameof(Generators))]
    public void Generate_RendersCustomContextMetadata(
        PromptPack.Services.Interfaces.IOutputGenerator generator
    )
    {
        var context = new RepositoryContext
        {
            HeaderText = "Review header",
            Instructions = "Follow these instructions",
            FullDirectoryPaths = ["README.md", "src/Program.cs"],
            SecurityFindings =
            [
                new SecurityFinding("appsettings.json", "generic secret assignment"),
            ],
            Files = [ProcessedFile.Create("src/Program.cs", ".", "code")],
        };

        var output = generator.Generate(context);

        output.ShouldContain("Review header");
        output.ShouldContain("Follow these instructions");
        output.ShouldContain("src/Program.cs");
        output.ShouldContain("generic secret assignment");
    }
}
