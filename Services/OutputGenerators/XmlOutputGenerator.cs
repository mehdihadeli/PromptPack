using System.Xml.Linq;
using PromptPack.Models;
using PromptPack.Services.Interfaces;

namespace PromptPack.Services.OutputGenerators;

public class XmlOutputGenerator : IOutputGenerator
{
    public string Style => "xml";
    
    public string Generate(RepositoryContext context)
    {
        var doc = new XDocument(
            new XElement("repository",
                new XElement("file_summary",
                    new XElement("generation_header", 
                        "This file is a merged representation of the entire codebase, " +
                        "combining all repository files into a single document for AI analysis."),
                    new XElement("purpose", 
                        "Enables comprehensive code review and analysis by AI tools like Claude, ChatGPT, and others."),
                    new XElement("generated_at", context.GeneratedAt.ToString("O")),
                    new XElement("file_count", context.FileCount),
                    new XElement("total_size", context.TotalSize)
                ),
                new XElement("directory_structure", context.GetDirectoryStructure()),
                new XElement("files",
                    context.Files.Select(f => 
                        new XElement("file",
                            new XAttribute("path", f.RelativePath),
                            new XAttribute("language", f.Language),
                            new XAttribute("size", f.Size),
                            new XCData(f.Content)
                        )
                    )
                )
            )
        );
        
        return $"<?xml version=\"1.0\" encoding=\"UTF-8\"?>\n{doc}";
    }
}