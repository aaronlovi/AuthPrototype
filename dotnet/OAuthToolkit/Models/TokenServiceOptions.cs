namespace OAuthToolkit.Models;

/// <summary>
/// Configuration options for the OAuth token validation services.
/// </summary>
/// <remarks>
/// This class follows the .NET options pattern and is designed to be populated from configuration 
/// (typically from appsettings.json) using <c>services.Configure&lt;TokenServiceOptions&gt;()</c>.
/// Services can access these settings by accepting an <c>IOptions&lt;TokenServiceOptions&gt;</c> parameter
/// in their constructors.
/// </remarks>
public class TokenServiceOptions {
    /// <summary>
    /// The default maximum number of concurrent token validation requests.
    /// </summary>
    public const int DefaultMaxConcurrentRequests = 20;

    /// <summary>
    /// Initializes a new instance of the <see cref="TokenServiceOptions"/> class with default values.
    /// </summary>
    public TokenServiceOptions() { }

    /// <summary>
    /// Gets or sets the client ID used to validate tokens with the OAuth2 provider.
    /// This value is required and must be set to a valid client ID.
    /// </summary>
    public string ClientId { get; init; } = string.Empty;

    /// <summary>
    /// Gets or sets the timeout in seconds for HTTP requests to the token validation endpoint.
    /// </summary>
    public int HttpClientTimeoutSeconds { get; init; }

    /// <summary>
    /// Gets or sets the maximum number of concurrent requests allowed to the token validation service.
    /// Defaults to 20. This helps control the load on the service and prevents excessive resource usage.
    /// </summary>
    public int MaxConcurrentRequests { get; init; } = DefaultMaxConcurrentRequests;
}
