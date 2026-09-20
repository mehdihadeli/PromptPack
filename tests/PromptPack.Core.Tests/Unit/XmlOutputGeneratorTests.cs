using System.Xml.Linq;
using PromptPack.Models;
using PromptPack.Services.OutputGenerators;

namespace PromptPack.Core.Tests.Unit;

public class XmlOutputGeneratorTests
{
    [Fact]
    public void Should_Produce_File_Metadata_And_Preserve_Markup_As_Content()
    {
        var context = new RepositoryContext
        {
            Files = [ProcessedFile.Create("src/Program.cs", ".", "if (value < 1) { }")],
        };

        var output = new XmlOutputGenerator().Generate(context);
        var document = XDocument.Parse(output);
        var file = document.Root!.Element("files")!.Element("file")!;

        file.Attribute("path")!.Value.ShouldBe("src/Program.cs");
        file.Attribute("language")!.Value.ShouldBe("csharp");
        file.Value.ShouldBe("if (value < 1) { }");
        document.Root.Element("file_summary")!.Element("file_count")!.Value.ShouldBe("1");
    }
}
