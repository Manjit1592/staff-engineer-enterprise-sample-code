using System.Text;
using Enterprise.RagService.Application.Interfaces;
using Enterprise.RagService.Domain.Entities;

namespace Enterprise.RagService.Application.Services;

public class DocumentChunkingService : IDocumentChunkingService
{
    public IReadOnlyList<RagDocumentChunk> ChunkDocument(
        RagDocument document,
        int chunkSize = 500,
        int overlap = 50)
    {
        if (chunkSize <= 0)
        {
            throw new ArgumentOutOfRangeException(nameof(chunkSize));
        }

        if (overlap < 0 || overlap >= chunkSize)
        {
            throw new ArgumentOutOfRangeException(nameof(overlap));
        }

        if (string.IsNullOrWhiteSpace(document.Content))
        {
            return Array.Empty<RagDocumentChunk>();
        }

        var normalizedContent = NormalizeContent(document.Content);

        var paragraphs = normalizedContent
            .Split(
                "\n\n",
                StringSplitOptions.RemoveEmptyEntries |
                StringSplitOptions.TrimEntries);

        var chunks = new List<RagDocumentChunk>();
        var currentParagraphs = new List<string>();
        var currentLength = 0;
        var chunkIndex = 0;

        foreach (var paragraph in paragraphs)
        {
            if (paragraph.Length > chunkSize)
            {
                if (currentParagraphs.Count > 0)
                {
                    AddChunk(
                        chunks,
                        document,
                        string.Join("\n\n", currentParagraphs),
                        chunkIndex++);

                    currentParagraphs.Clear();
                    currentLength = 0;
                }

                var paragraphChunks = SplitLargeParagraph(
                    paragraph,
                    chunkSize,
                    overlap);

                foreach (var paragraphChunk in paragraphChunks)
                {
                    AddChunk(
                        chunks,
                        document,
                        paragraphChunk,
                        chunkIndex++);
                }

                continue;
            }

            var additionalLength =
                currentParagraphs.Count == 0
                    ? paragraph.Length
                    : paragraph.Length + 2;

            if (currentLength + additionalLength <= chunkSize)
            {
                currentParagraphs.Add(paragraph);
                currentLength += additionalLength;
                continue;
            }

            AddChunk(
                chunks,
                document,
                string.Join("\n\n", currentParagraphs),
                chunkIndex++);

            var overlapText = BuildOverlap(
                currentParagraphs,
                overlap);

            currentParagraphs.Clear();
            currentLength = 0;

            if (!string.IsNullOrWhiteSpace(overlapText))
            {
                currentParagraphs.Add(overlapText);
                currentLength = overlapText.Length;
            }

            if (currentLength > 0)
            {
                currentLength += 2;
            }

            currentParagraphs.Add(paragraph);
            currentLength += paragraph.Length;
        }

        if (currentParagraphs.Count > 0)
        {
            AddChunk(
                chunks,
                document,
                string.Join("\n\n", currentParagraphs),
                chunkIndex);
        }

        return chunks;
    }

    private static string NormalizeContent(string content)
    {
        return content
            .Replace("\r\n", "\n")
            .Replace("\r", "\n")
            .Trim();
    }

    private static void AddChunk(
        List<RagDocumentChunk> chunks,
        RagDocument document,
        string content,
        int chunkIndex)
    {
        if (string.IsNullOrWhiteSpace(content))
        {
            return;
        }

        chunks.Add(new RagDocumentChunk
        {
            Id = Guid.NewGuid(),
            DocumentId = document.Id,
            Content = content.Trim(),
            ChunkIndex = chunkIndex,
            TenantId = document.TenantId,
            Source = document.Source
        });
    }

    private static IReadOnlyList<string> SplitLargeParagraph(
        string paragraph,
        int chunkSize,
        int overlap)
    {
        var chunks = new List<string>();

        var step = chunkSize - overlap;

        for (var start = 0;
             start < paragraph.Length;
             start += step)
        {
            var length = Math.Min(
                chunkSize,
                paragraph.Length - start);

            chunks.Add(
                paragraph.Substring(start, length).Trim());

            if (start + length >= paragraph.Length)
            {
                break;
            }
        }

        return chunks;
    }

    private static string BuildOverlap(
        IReadOnlyList<string> paragraphs,
        int overlap)
    {
        if (overlap <= 0 || paragraphs.Count == 0)
        {
            return string.Empty;
        }

        var builder = new StringBuilder();

        for (var i = paragraphs.Count - 1; i >= 0; i--)
        {
            var paragraph = paragraphs[i];

            if (builder.Length == 0)
            {
                builder.Insert(0, paragraph);
            }
            else
            {
                builder.Insert(0, paragraph + "\n\n");
            }

            if (builder.Length >= overlap)
            {
                break;
            }
        }

        var result = builder.ToString();

        if (result.Length <= overlap)
        {
            return result;
        }

        return result[^overlap..];
    }
}