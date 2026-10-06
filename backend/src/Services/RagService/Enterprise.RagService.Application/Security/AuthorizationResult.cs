namespace Enterprise.RagService.Application.Security;

public class AuthorizationResult
{
    public bool IsAllowed { get; set; }

    public string ErrorCode { get; set; } = string.Empty;

    public string Reason { get; set; } = string.Empty;

    public static AuthorizationResult Allow()
    {
        return new AuthorizationResult
        {
            IsAllowed = true
        };
    }

    public static AuthorizationResult Deny(
        string errorCode,
        string reason)
    {
        return new AuthorizationResult
        {
            IsAllowed = false,
            ErrorCode = errorCode,
            Reason = reason
        };
    }
}