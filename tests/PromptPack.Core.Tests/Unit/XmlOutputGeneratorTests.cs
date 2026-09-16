using System.Xml.Linq;
using PromptPack.Models;
using PromptPack.Services.OutputGenerators;

namespace PromptPack.Core.Tests.Unit;

public class XmlOutputGeneratorTests
{
    [Fact]
    public void Generate_ProducesFileMetadataAndPreservesMarkupAsContent()
    {
        var context = new RepositoryContext
        {
            Files = [ProcessedFile.Create("src/Program.cs", ".", "if (value < 1) { }")],
        };

        var output = new XmlOutputGenerator().Generate(context);
        var document = XDocument.Parse(output);
        var file = document.Root!.Element("files")!.Element("file")!;

        Assert.Equal("src/Program.cs", file.Attribute("path")!.Value);
        Assert.Equal("csharp", file.Attribute("language")!.Value);
        Assert.Equal("if (value < 1) { }", file.Value);
        Assert.Equal("1", document.Root.Element("file_summary")!.Element("file_count")!.Value);
    }
}
