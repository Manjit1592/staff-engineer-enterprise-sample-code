using Enterprise.RagService.Application.Interfaces;
using Enterprise.RagService.Application.Models;

namespace Enterprise.RagService.Infrastructure;

public class DevelopmentMcpToolRegistry : IMcpToolRegistry
{
    private readonly IReadOnlyList<McpToolDefinition> _tools =
        new List<McpToolDefinition>
        {
            new()
            {
                Name = "get_inventory",

                Description =
                    "Gets the current inventory availability " +
                    "for a product.",

                RequiredArguments =
                    new[]
                    {
                        "productId"
                    },

                InputSchema =
                    new McpToolSchema
                    {
                        Type = "object",
                        Properties =
                            new Dictionary<string, string>
                            {
                                ["productId"] = "string"
                            },
                        Required =
                            new[]
                            {
                                "productId"
                            }
                    },

                OutputSchema =
                    new McpToolSchema
                    {
                        Type = "object",
                        Properties =
                            new Dictionary<string, string>
                            {
                                ["productId"] = "string",
                                ["availableQuantity"] = "integer",
                                ["warehouse"] = "string",
                                ["isAvailable"] = "boolean"
                            }
                    },

                Permissions =
                    new[]
                    {
                        new McpToolPermission
                        {
                            ToolName = "get_inventory",
                            Role = "Admin",
                            Allowed = true
                        },
                        new McpToolPermission
                        {
                            ToolName = "get_inventory",
                            Role = "Viewer",
                            Allowed = true
                        }
                    },

                ExecutionPolicy =
                    new McpToolExecutionPolicy
                    {
                        TimeoutSeconds = 10,
                        MaxRetryCount = 2,
                        EnableRetry = true
                    },

                SupportedIntents =
                    new[]
                    {
                        AiIntent.LiveData,
                        AiIntent.Hybrid
                    }
            },

            new()
            {
                Name = "get_order",

                Description =
                    "Gets the current status of an order.",

                RequiredArguments =
                    new[]
                    {
                        "orderId"
                    },

                InputSchema =
                    new McpToolSchema
                    {
                        Type = "object",
                        Properties =
                            new Dictionary<string, string>
                            {
                                ["orderId"] = "string"
                            },
                        Required =
                            new[]
                            {
                                "orderId"
                            }
                    },

                OutputSchema =
                    new McpToolSchema
                    {
                        Type = "object",
                        Properties =
                            new Dictionary<string, string>
                            {
                                ["orderId"] = "string",
                                ["status"] = "string",
                                ["tenantId"] = "string"
                            }
                    },

                Permissions =
                    new[]
                    {
                        new McpToolPermission
                        {
                            ToolName = "get_order",
                            Role = "Admin",
                            Allowed = true
                        },
                        new McpToolPermission
                        {
                            ToolName = "get_order",
                            Role = "Viewer",
                            Allowed = true
                        }
                    },

                ExecutionPolicy =
                    new McpToolExecutionPolicy
                    {
                        TimeoutSeconds = 10,
                        MaxRetryCount = 2,
                        EnableRetry = true
                    },

                SupportedIntents =
                    new[]
                    {
                        AiIntent.LiveData
                    }
            }
        };

    public IReadOnlyList<McpToolDefinition> GetTools()
    {
        return _tools;
    }

    public McpToolDefinition? GetTool(
        string toolName)
    {
        return _tools.FirstOrDefault(
            tool =>
                string.Equals(
                    tool.Name,
                    toolName,
                    StringComparison.OrdinalIgnoreCase));
    }

    public McpToolDefinition? GetToolForIntent(
        AiIntent intent)
    {
        return _tools.FirstOrDefault(
            tool =>
                tool.SupportedIntents.Contains(intent));
    }
}