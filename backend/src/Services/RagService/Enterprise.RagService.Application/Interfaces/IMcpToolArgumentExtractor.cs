using Enterprise.RagService.Application.Models;

namespace Enterprise.RagService.Application.Interfaces;

public interface IMcpToolArgumentExtractor
{
    McpArgumentExtractionResult Extract(
        string toolName,
        string query);
}