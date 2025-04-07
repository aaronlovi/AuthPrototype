using System;
using System.Net.Http;
using System.Threading.Tasks;
using AuthPrototype.Models;
using AuthPrototype.Utilities;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Polly;
using Polly.CircuitBreaker;
using Polly.RateLimit;
using Polly.Retry;

namespace AuthPrototype.Services;

/// <summary>
/// Provides functionality for validating OAuth2 access tokens.
/// </summary>
/// <remarks>
/// The <see cref="TokenService"/> class is responsible for validating access tokens by making requests to the OAuth2 token info endpoint.
/// It uses Polly policies for retry, circuit breaker, and rate limiting to handle transient faults and rate limits.
/// The service also caches validated tokens to improve performance and reduce the number of external requests.
/// </remarks>
public class TokenService
{
    private const int MaxRetryAttempts = 3;
    private const int MaxExceptionsBeforeCircuitBreak = 2;
    private const int MaxRequestsPerMinute = 100;
    private static readonly TimeSpan[] RetryDelays = [ TimeSpan.FromSeconds(1), TimeSpan.FromSeconds(2), TimeSpan.FromSeconds(4) ];

    private static readonly AsyncRetryPolicy<HttpResponseMessage> RetryPolicy =
        PollyExtensions.GetRetryPolicy<HttpResponseMessage>(MaxRetryAttempts, RetryDelays, r => !r.IsSuccessStatusCode);
    private static readonly AsyncCircuitBreakerPolicy<HttpResponseMessage> CircuitBreakerPolicy =
        PollyExtensions.GetCircuitBreakerPolicy<HttpResponseMessage>(MaxExceptionsBeforeCircuitBreak, TimeSpan.FromMinutes(1), r => !r.IsSuccessStatusCode);
    private static readonly AsyncRateLimitPolicy<HttpResponseMessage> RateLimitPolicy =
        PollyExtensions.GetRateLimitPolicy<HttpResponseMessage>(MaxRequestsPerMinute, TimeSpan.FromMinutes(1));

    private readonly TokenHttpClient _tokenHttpClient;
    private readonly TokenValidator _tokenValidator;
    private readonly ITokenCache _tokenCache;
    private readonly ILogger<TokenService> _logger;

    public TokenService(HttpClient httpClient, IConfiguration config, ITokenCache tokenCache, ILogger<TokenService> logger)
    {
        _tokenHttpClient = new TokenHttpClient(httpClient);
        _tokenValidator = new TokenValidator(config["CLIENT_ID"] ?? throw new InvalidOperationException("CLIENT_ID not set"));
        _tokenCache = tokenCache;
        _logger = logger;
    }

    /// <summary>
    /// Validates the provided access token by making a request to the OAuth2 token info endpoint.
    /// </summary>
    /// <param name="accessToken">The access token to validate.</param>
    /// <returns>
    /// A <see cref="ValidateAccessTokenResponse"/> containing the token's expiration time, email, and provider user ID if the token is valid;
    /// otherwise, <see cref="ValidateAccessTokenResponse.Empty"/>.
    /// </returns>
    /// <exception cref="InvalidOperationException">Thrown if the CLIENT_ID is not set in the configuration.</exception>
    /// <remarks>
    /// This method sends a GET request to the Google OAuth2 token info endpoint to validate the access token.
    /// If the token is valid, it returns a response with the token's expiration time, email, and provider user ID.
    /// If the token is invalid or expired, it returns an empty response.
    /// </remarks>
    public async Task<ValidateAccessTokenResponse> ValidateAccessToken(string accessToken) {
        using var accessTokenLogContext = _logger.BeginScope("AccessToken: {AccessToken}", accessToken);

        _logger.LogInformation("ValidateAccessToken");

        var cachedResponse = _tokenCache.Get(accessToken);
        if (cachedResponse is not null) {
            _logger.LogInformation("ValidateAccessToken - found in cache");
            return cachedResponse;
        }

        _logger.LogInformation("ValidateAccessToken - not found in cache, validating with Google");

        var context = new Context().WithLogger(_logger);
        var response = await Policy.WrapAsync(RateLimitPolicy, RetryPolicy, CircuitBreakerPolicy)
            .ExecuteAsync((ctx) => _tokenHttpClient.GetTokenInfoAsync(accessToken), context);

        if (!response.IsSuccessStatusCode) {
            _logger.LogWarning("ValidateAccessToken - failed - Status Code {StatusCode}", response.StatusCode);
            return ValidateAccessTokenResponse.Empty;
        }

        var responseContent = await response.Content.ReadAsStringAsync();
        var tokenInfo = Conventions.Deserialize<TokenInfoResponse>(responseContent);

        var validateResponse = _tokenValidator.ValidateTokenInfo(tokenInfo, responseContent, _logger);

        if (validateResponse.IsValid)
            _tokenCache.Set(accessToken, validateResponse, TimeSpan.FromSeconds(tokenInfo!.ExpiresIn));

        return validateResponse;
    }
}
