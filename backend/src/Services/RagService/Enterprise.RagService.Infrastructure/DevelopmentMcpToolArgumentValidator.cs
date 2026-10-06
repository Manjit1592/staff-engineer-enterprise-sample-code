using Enterprise.RagService.Application.Interfaces;
using Enterprise.RagService.Application.Models;

namespace Enterprise.RagService.Infrastructure;

public class DevelopmentMcpToolArgumentValidator
    : IMcpToolArgumentValidator
{
    public McpToolValidationResult Validate(
        McpToolDefinition tool,
        IReadOnlyDictionary<string, object?> arguments)
    {
        if (tool is null)
        {
            return new McpToolValidationResult
            {
                IsValid = false,
                Error = "Tool definition cannot be null."
            };
        }

        if (arguments is null)
        {
            return new McpToolValidationResult
            {
                IsValid = false,
                Error = "Arguments cannot be null."
            };
        }

        foreach (var requiredArgument
                 in tool.RequiredArguments)
        {
            if (!arguments.TryGetValue(
                    requiredArgument,
                    out var value) ||
                value is null ||
                string.IsNullOrWhiteSpace(
                    value.ToString()))
            {
                return new McpToolValidationResult
                {
                    IsValid = false,
                    Error =
                        $"Required argument '{requiredArgument}' " +
                        "is missing."
                };
            }
        }

        if (string.Equals(
                tool.Name,
                "get_inventory",
                StringComparison.OrdinalIgnoreCase))
        {
            var productId =
                arguments["productId"]?.ToString();

            if (string.IsNullOrWhiteSpace(productId) ||
                !productId.StartsWith(
                    "P",
                    StringComparison.OrdinalIgnoreCase))
            {
                return new McpToolValidationResult
                {
                    IsValid = false,
                    Error =
                        "productId must be a valid product identifier."
                };
            }
        }

        if (string.Equals(
                tool.Name,
                "get_order",
                StringComparison.OrdinalIgnoreCase))
        {
            var orderId =
                arguments["orderId"]?.ToString();

            if (string.IsNullOrWhiteSpace(orderId) ||
                !orderId.StartsWith(
                    "O",
                    StringComparison.OrdinalIgnoreCase))
            {
                return new McpToolValidationResult
                {
                    IsValid = false,
                    Error =
                        "orderId must be a valid order identifier."
                };
            }
        }

        return new McpToolValidationResult
        {
            IsValid = true
        };
    }
}