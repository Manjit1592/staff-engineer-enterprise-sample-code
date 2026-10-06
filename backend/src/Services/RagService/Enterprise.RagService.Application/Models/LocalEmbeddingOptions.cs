namespace Enterprise.RagService.Application.Models;

public class LocalEmbeddingOptions
{
    public string Endpoint { get; set; } =
        "http://localhost:11434";

    public string Model { get; set; } =
        "nomic-embed-text";

    public int Dimensions { get; set; } = 768;

    public int TimeoutSeconds { get; set; } = 60;
}