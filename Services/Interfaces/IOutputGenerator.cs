using PromptPack.Models;

namespace PromptPack.Services.Interfaces;

public interface IOutputGenerator
{
    string Style { get; }
    string Generate(RepositoryContext context);
}