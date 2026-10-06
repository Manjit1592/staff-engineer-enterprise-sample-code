using System.Text;
using Enterprise.RagService.Application.Interfaces;
using Enterprise.RagService.Application.Models;

namespace Enterprise.RagService.Application.Services;

public class RagContextBuilder : IRagContextBuilder
{
    public RagContext Build(
        IReadOnlyList<RagSearchResult> searchResults)
    {
        if (searchResults.Count == 0)
        {
            return new RagContext();
        }

        var builder = new StringBuilder();

        foreach (var result in searchResults.OrderBy(
                     result => result.ChunkIndex))
        {
            builder.AppendLine(
                $"[Source: Document {result.DocumentId}, " +
                $"Chunk {result.ChunkIndex}]");

            builder.AppendLine(result.Content);
            builder.AppendLine();
        }

        return new RagContext
        {
            Content = builder.ToString().Trim(),
            Sources = searchResults
        };
    }
}