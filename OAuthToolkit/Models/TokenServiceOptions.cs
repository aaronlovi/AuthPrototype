namespace OAuthToolkit.Models;

/// <summary>
/// Options for the TokenService.
/// </summary>
/// <param name="ClientId">
/// The client ID used to validate tokens with the OAuth2 provider. This value is required and must be set to a valid client ID.
/// </param>
/// <param name="MaxConcurrentRequests">
/// The maximum number of concurrent requests allowed to the token validation service. Defaults to 20.
/// This helps control the load on the service and prevents excessive resource usage.
/// </param>
public class TokenServiceOptions {
    public const int DefaultMaxConcurrentRequests = 20;

    public TokenServiceOptions() { }

    public string ClientId { get; init; } = string.Empty;
    public int HttpClientTimeoutSeconds { get; init; }
    public int MaxConcurrentRequests { get; init; } = DefaultMaxConcurrentRequests;
}
