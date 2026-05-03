using PromptPack.Models;

namespace PromptPack.Services.Interfaces;

public interface IContentProcessor
{
    Task<ProcessedFile> ProcessFileAsync(string filePath, string baseDirectory, bool removeComments, bool compress);
}