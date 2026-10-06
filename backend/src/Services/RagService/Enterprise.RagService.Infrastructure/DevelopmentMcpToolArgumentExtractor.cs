using System.Text.RegularExpressions;
using Enterprise.RagService.Application.Interfaces;
using Enterprise.RagService.Application.Models;

namespace Enterprise.RagService.Infrastructure;

public class DevelopmentMcpToolArgumentExtractor
    : IMcpToolArgumentExtractor
{
    public McpArgumentExtractionResult Extract(
        string toolName,
        string query)
    {
        if (string.IsNullOrWhiteSpace(toolName))
        {
            return new McpArgumentExtractionResult
            {
                Success = false,
                Error = "ToolName cannot be empty."
            };
        }

        if (string.IsNullOrWhiteSpace(query))
        {
            return new McpArgumentExtractionResult
            {
                Success = false,
                Error = "Query cannot be empty."
            };
        }

        if (string.Equals(
                toolName,
                "get_inventory",
                StringComparison.OrdinalIgnoreCase))
        {
            var match =
                Regex.Match(
                    query,
                    @"\bP\d+\b",
                    RegexOptions.IgnoreCase);

            if (!match.Success)
            {
                return new McpArgumentExtractionResult
                {
                    Success = false,
                    Error =
                        "A productId such as P100 was not found " +
                        "in the request."
                };
            }

            return new McpArgumentExtractionResult
            {
                Success = true,
                Arguments =
                    new Dictionary<string, object?>
                    {
                        ["productId"] =
                            match.Value.ToUpperInvariant()
                    }
            };
        }

        if (string.Equals(
                toolName,
                "get_order",
                StringComparison.OrdinalIgnoreCase))
        {
            var match =
                Regex.Match(
                    query,
                    @"\bO\d+\b",
                    RegexOptions.IgnoreCase);

            if (!match.Success)
            {
                return new McpArgumentExtractionResult
                {
                    Success = false,
                    Error =
                        "An orderId such as O100 was not found " +
                        "in the request."
                };
            }

            return new McpArgumentExtractionResult
            {
                Success = true,
                Arguments =
                    new Dictionary<string, object?>
                    {
                        ["orderId"] =
                            match.Value.ToUpperInvariant()
                    }
            };
        }

        return new McpArgumentExtractionResult
        {
            Success = false,
            Error =
                $"Argument extraction is not supported " +
                $"for tool '{toolName}'."
        };
    }
}