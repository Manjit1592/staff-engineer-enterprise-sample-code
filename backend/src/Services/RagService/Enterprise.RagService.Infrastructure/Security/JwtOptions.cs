namespace Enterprise.RagService.Infrastructure.Security;

public class JwtOptions
{
    public string Issuer { get; set; } =
        "EnterpriseOrderPlatform";

    public string Audience { get; set; } =
        "EnterpriseOrderPlatform.Api";

    public string SigningKey { get; set; } =
        string.Empty;

    public int ExpirationMinutes { get; set; } = 60;
}