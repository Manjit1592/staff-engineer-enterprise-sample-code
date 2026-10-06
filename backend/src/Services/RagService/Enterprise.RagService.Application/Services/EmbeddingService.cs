using System.Security.Cryptography;
using System.Text;
using Enterprise.RagService.Application.Interfaces;

namespace Enterprise.RagService.Application.Services;

public class EmbeddingService : IEmbeddingService
{
    private const int EmbeddingDimensions = 128;

    public Task<float[]> GenerateEmbeddingAsync(
        string text,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(text))
        {
            throw new ArgumentException(
                "Text cannot be empty.",
                nameof(text));
        }

        cancellationToken.ThrowIfCancellationRequested();

        var embedding = new float[EmbeddingDimensions];

        var bytes = SHA256.HashData(
            Encoding.UTF8.GetBytes(text));

        for (var i = 0; i < embedding.Length; i++)
        {
            var byteValue = bytes[i % bytes.Length];

            embedding[i] =
                (byteValue / 255f) * 2f - 1f;
        }

        return Task.FromResult(embedding);
    }
}