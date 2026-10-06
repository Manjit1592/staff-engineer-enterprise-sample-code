using Enterprise.RagService.Application.Models;

namespace Enterprise.RagService.Application.Interfaces;

public interface IIntentClassifier
{
    Task<IntentClassificationResult> ClassifyAsync(
        string query,
        CancellationToken cancellationToken = default);
}