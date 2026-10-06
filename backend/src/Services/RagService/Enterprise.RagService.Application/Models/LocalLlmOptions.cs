namespace Enterprise.RagService.Application.Models;

public class LocalLlmOptions
{
    public string Endpoint { get; set; } =
        "http://localhost:11434";

    public string Model { get; set; } =
        "llama3.2";

    public double Temperature { get; set; } = 0.0;

    public int TimeoutSeconds { get; set; } = 60;
}