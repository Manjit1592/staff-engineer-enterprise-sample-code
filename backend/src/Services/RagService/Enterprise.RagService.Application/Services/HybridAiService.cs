using Enterprise.RagService.Application.Interfaces;
using Enterprise.RagService.Application.Models;

namespace Enterprise.RagService.Application.Services;

public class HybridAiService : IHybridAiService
{
    private readonly IRagAnswerService _ragAnswerService;
    private readonly IMcpToolService _mcpToolService;
    private readonly IMcpToolRegistry _mcpToolRegistry;
    private readonly IMcpToolArgumentExtractor _argumentExtractor;

    public HybridAiService(
        IRagAnswerService ragAnswerService,
        IMcpToolService mcpToolService,
        IMcpToolRegistry mcpToolRegistry,
        IMcpToolArgumentExtractor argumentExtractor)
    {
        _ragAnswerService = ragAnswerService;
        _mcpToolService = mcpToolService;
        _mcpToolRegistry = mcpToolRegistry;
        _argumentExtractor = argumentExtractor;
    }

    public async Task<HybridAiResponse> ExecuteAsync(
        HybridAiRequest request,
        CancellationToken cancellationToken = default)
    {
        if (request is null)
        {
            throw new ArgumentNullException(nameof(request));
        }

        if (string.IsNullOrWhiteSpace(request.Query))
        {
            throw new ArgumentException(
                "Query cannot be empty.",
                nameof(request.Query));
        }

        if (string.IsNullOrWhiteSpace(request.TenantId))
        {
            throw new ArgumentException(
                "TenantId cannot be empty.",
                nameof(request.TenantId));
        }

        if (request.TopK <= 0)
        {
            throw new ArgumentOutOfRangeException(
                nameof(request.TopK));
        }

        var inventoryTool =
            _mcpToolRegistry.GetTool("get_inventory");

        if (inventoryTool is null)
        {
            throw new InvalidOperationException(
                "The get_inventory MCP tool is not registered.");
        }

        var extraction =
            _argumentExtractor.Extract(
                inventoryTool.Name,
                request.Query);

        if (!extraction.Success)
        {
            throw new InvalidOperationException(
                extraction.Error);
        }

        var ragTask =
            _ragAnswerService.AskAsync(
                request.Query,
                request.TenantId,
                request.TopK,
                cancellationToken);

        var mcpTask =
            _mcpToolService.ExecuteAsync(
                new McpToolRequest
                {
                    ToolName = inventoryTool.Name,
                    Query = request.Query,
                    TenantId = request.TenantId,
                    Arguments = extraction.Arguments
                },
                cancellationToken);

        await Task.WhenAll(
            ragTask,
            mcpTask);

        var ragResult = await ragTask;
        var mcpResult = await mcpTask;

        return new HybridAiResponse
        {
            RagAnswer = ragResult.Answer,
            McpResult = mcpResult.Result,
            Sources = ragResult.Sources,
            Citations = ragResult.Citations
        };
    }
}