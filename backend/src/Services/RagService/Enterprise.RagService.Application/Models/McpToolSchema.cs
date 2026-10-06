
namespace Enterprise.RagService.Application.Models;

public class McpToolSchema
{
    public string Type { get; set; } = "object";

    public IReadOnlyDictionary<string, string> Properties { get; set; }
        = new Dictionary<string, string>();

    public IReadOnlyList<string> Required { get; set; }
        = Array.Empty<string>();
}