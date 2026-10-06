using Enterprise.RagService.Application.Models;

namespace Enterprise.RagService.Application.Interfaces;

public interface IHybridAnswerSynthesizer
{
    Task<string> SynthesizeAsync(
        HybridAnswerSynthesisRequest request,
        CancellationToken cancellationToken = default);
}