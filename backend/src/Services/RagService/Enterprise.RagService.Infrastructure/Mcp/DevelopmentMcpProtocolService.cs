using Enterprise.RagService.Application.Identity;
using Enterprise.RagService.Application.Interfaces;
using Enterprise.RagService.Application.Mcp;
using Enterprise.RagService.Application.Models;

namespace Enterprise.RagService.Infrastructure.Mcp;

public class DevelopmentMcpProtocolService
    : IMcpProtocolService
{
    private readonly IMcpToolService _toolService;
    private readonly IMcpUserContext _userContext;

    public DevelopmentMcpProtocolService(
        IMcpToolService toolService,
        IMcpUserContext userContext)
    {
        _toolService = toolService;
        _userContext = userContext;
    }

    public async Task<McpProtocolResponse> InvokeAsync(
        McpProtocolRequest request,
        CancellationToken cancellationToken = default)
    {
        if (request is null)
        {
            throw new ArgumentNullException(nameof(request));
        }

        if (!_userContext.IsAuthenticated)
        {
            return new McpProtocolResponse
            {
                Success = false,
                ToolName = request.ToolName,
                Result =
                    "The MCP request is not authenticated.",
                ErrorCode = "UNAUTHENTICATED"
            };
        }

        var response =
            await _toolService.ExecuteAsync(
                new McpToolRequest
                {
                    ToolName =
                        request.ToolName,

                    Query =
                        request.Query,

                    TenantId =
                        _userContext.TenantId,

                    UserId =
                        _userContext.UserId,

                    Role =
                        _userContext.Role,

                    CorrelationId =
                        Guid.NewGuid().ToString("N"),

                    Arguments =
                        request.Arguments
                },
                cancellationToken);

        var failure =
            response.Data as McpToolFailure;

        return new McpProtocolResponse
        {
            Success =
                response.Success,

            ToolName =
                response.ToolName,

            Result =
                response.Result,

            Data =
                response.Data,

            ErrorCode =
                failure?.ErrorCode
                ?? string.Empty
        };
    }
}