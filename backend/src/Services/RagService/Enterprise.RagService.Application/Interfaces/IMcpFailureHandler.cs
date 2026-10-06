using Enterprise.RagService.Application.Models;

namespace Enterprise.RagService.Application.Interfaces;

public interface IMcpFailureHandler
{
    McpToolResponse Handle(
        string toolName,
        string errorCode,
        string message);
}