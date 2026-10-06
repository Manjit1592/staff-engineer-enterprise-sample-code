using Enterprise.RagService.Application.Interfaces;
using Enterprise.RagService.Application.Models;

namespace Enterprise.RagService.Application.Services;

public class AiOrchestrator : IAiOrchestrator
{
    private readonly IRagAnswerService _ragAnswerService;
    private readonly IIntentClassifier _intentClassifier;
    private readonly IMcpToolService _mcpToolService;
    private readonly IHybridAiService _hybridAiService;
    private readonly IMcpToolRegistry _mcpToolRegistry;
    private readonly IHybridAnswerSynthesizer _hybridAnswerSynthesizer;
    private readonly IMcpToolArgumentExtractor _argumentExtractor;

    public AiOrchestrator(
        IRagAnswerService ragAnswerService,
        IIntentClassifier intentClassifier,
        IMcpToolService mcpToolService,
        IHybridAiService hybridAiService,
        IMcpToolRegistry mcpToolRegistry,
        IHybridAnswerSynthesizer hybridAnswerSynthesizer,
        IMcpToolArgumentExtractor argumentExtractor)
    {
        _ragAnswerService = ragAnswerService;
        _intentClassifier = intentClassifier;
        _mcpToolService = mcpToolService;
        _hybridAiService = hybridAiService;
        _mcpToolRegistry = mcpToolRegistry;
        _hybridAnswerSynthesizer = hybridAnswerSynthesizer;
        _argumentExtractor = argumentExtractor;
    }

    public async Task<AiOrchestratorResponse> ExecuteAsync(
        AiOrchestratorRequest request,
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

        var intent =
            await _intentClassifier.ClassifyAsync(
                request.Query,
                cancellationToken);

        switch (intent.Intent)
        {
            case AiIntent.LiveData:
            {
                var toolSelection =
                    SelectTool(
                        intent.Intent,
                        request.Query);

                var extraction =
                    _argumentExtractor.Extract(
                        toolSelection.ToolName,
                        request.Query);

                if (!extraction.Success)
                {
                    return new AiOrchestratorResponse
                    {
                        Query =
                            request.Query,

                        TenantId =
                            request.TenantId,

                        Answer =
                            $"Unable to determine the required " +
                            $"tool arguments: {extraction.Error}"
                    };
                }

                var mcpResponse =
                    await _mcpToolService.ExecuteAsync(
                        new McpToolRequest
                        {
                            ToolName =
                                toolSelection.ToolName,

                            Query =
                                request.Query,

                            TenantId =
                                request.TenantId,

                            UserId =
                                "development-user",

                            Role =
                                "Viewer",

                            CorrelationId =
                                Guid.NewGuid().ToString("N"),

                            Arguments =
                                extraction.Arguments
                        },
                        cancellationToken);

                return new AiOrchestratorResponse
                {
                    Query =
                        request.Query,

                    TenantId =
                        request.TenantId,

                    Answer =
                        mcpResponse.Result
                };
            }

            case AiIntent.Action:
            {
                return new AiOrchestratorResponse
                {
                    Query =
                        request.Query,

                    TenantId =
                        request.TenantId,

                    Answer =
                        "No MCP action tool is currently available " +
                        "for this request."
                };
            }

            case AiIntent.Hybrid:
            {
                var hybridResponse =
                    await _hybridAiService.ExecuteAsync(
                        new HybridAiRequest
                        {
                            Query =
                                request.Query,

                            TenantId =
                                request.TenantId,

                            TopK =
                                request.TopK
                        },
                        cancellationToken);

                var synthesizedAnswer =
                    await _hybridAnswerSynthesizer.SynthesizeAsync(
                        new HybridAnswerSynthesisRequest
                        {
                            Query =
                                request.Query,

                            RagAnswer =
                                hybridResponse.RagAnswer,

                            McpResult =
                                hybridResponse.McpResult
                        },
                        cancellationToken);

                return new AiOrchestratorResponse
                {
                    Query =
                        request.Query,

                    TenantId =
                        request.TenantId,

                    Answer =
                        synthesizedAnswer,

                    Sources =
                        hybridResponse.Sources,

                    Citations =
                        hybridResponse.Citations
                };
            }

            case AiIntent.Knowledge:
            default:
            {
                var result =
                    await _ragAnswerService.AskAsync(
                        request.Query,
                        request.TenantId,
                        request.TopK,
                        cancellationToken);

                return new AiOrchestratorResponse
                {
                    Query =
                        request.Query,

                    TenantId =
                        request.TenantId,

                    Answer =
                        result.Answer,

                    Sources =
                        result.Sources,

                    Citations =
                        result.Citations
                };
            }
        }
    }

    private McpToolSelection SelectTool(
        AiIntent intent,
        string query)
    {
        var normalizedQuery =
            query.Trim().ToLowerInvariant();

        var tools =
            _mcpToolRegistry.GetTools()
                .Where(tool =>
                    tool.SupportedIntents.Contains(intent))
                .ToList();

        if (intent == AiIntent.LiveData)
        {
            if (normalizedQuery.Contains("order") ||
                RegexLikeOrderId(normalizedQuery))
            {
                var orderTool =
                    tools.FirstOrDefault(
                        tool =>
                            string.Equals(
                                tool.Name,
                                "get_order",
                                StringComparison.OrdinalIgnoreCase));

                if (orderTool is not null)
                {
                    return new McpToolSelection
                    {
                        ToolName =
                            orderTool.Name,

                        Reason =
                            "Order-related live-data request " +
                            "was routed to the order tool."
                    };
                }
            }

            var inventoryTool =
                tools.FirstOrDefault(
                    tool =>
                        string.Equals(
                            tool.Name,
                            "get_inventory",
                            StringComparison.OrdinalIgnoreCase));

            if (inventoryTool is not null)
            {
                return new McpToolSelection
                {
                    ToolName =
                        inventoryTool.Name,

                    Reason =
                        "Inventory-related live-data request " +
                        "was routed to the inventory tool."
                };
            }
        }

        var defaultTool =
            tools.FirstOrDefault();

        if (defaultTool is not null)
        {
            return new McpToolSelection
            {
                ToolName =
                    defaultTool.Name,

                Reason =
                    $"Tool '{defaultTool.Name}' selected " +
                    $"for intent '{intent}'."
            };
        }

        throw new InvalidOperationException(
            $"No MCP tool is available for intent '{intent}'.");
    }

    private static bool RegexLikeOrderId(
        string query)
    {
        return System.Text.RegularExpressions.Regex.IsMatch(
            query,
            @"\bo\d+\b",
            System.Text.RegularExpressions.RegexOptions.IgnoreCase);
    }
}