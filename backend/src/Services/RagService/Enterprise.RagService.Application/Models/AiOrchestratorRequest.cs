namespace Enterprise.RagService.Application.Models;

public class AiOrchestratorRequest
{
    public string Query { get; set; } = string.Empty;

    public string TenantId { get; set; } = string.Empty;

    public int TopK { get; set; } = 5;
}