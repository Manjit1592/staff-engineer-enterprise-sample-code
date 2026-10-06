using Enterprise.RagService.Application.Interfaces;
using Enterprise.RagService.Application.Models;

namespace Enterprise.RagService.Infrastructure;

public class DevelopmentMcpFailureHandler
    : IMcpFailureHandler
{
    public McpToolResponse Handle(
        string toolName,
        string errorCode,
        string message)
    {
        return new McpToolResponse
        {
            Success = false,
            ToolName = toolName,
            Result =
                "The requested MCP tool could not be executed.",
            Data = new McpToolFailure
            {
                ErrorCode = errorCode,
                Message = message
            }
        };
    }
}