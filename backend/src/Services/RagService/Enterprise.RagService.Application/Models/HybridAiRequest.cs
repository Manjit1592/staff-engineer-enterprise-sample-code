namespace Enterprise.RagService.Application.Models;

public class HybridAiRequest
{
    public string Query { get; set; } = string.Empty;

    public string TenantId { get; set; } = string.Empty;

    public int TopK { get; set; } = 5;
}