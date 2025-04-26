using System;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using OAuthToolkit.Contracts;
using OAuthToolkit.Models;
using OAuthToolkit.Shared;
using Polly;
using Polly.CircuitBreaker;
using Polly.RateLimit;
using Polly.Retry;

namespace OAuthToolkit.Services;

/// <summary>
/// Provides functionality for validating OAuth2 access tokens.
/// </summary>
/// <remarks>
/// The <see cref="TokenService"/> class is responsible for validating access tokens by making requests to the OAuth2 token info endpoint.
/// It uses Polly policies for retry, circuit breaker, and rate limiting to handle transient faults and rate limits.
/// The service also caches validated tokens to improve performance and reduce the number of external requests.
/// </remarks>
internal class TokenService : ITokenService {
    private const int DefaultMaxRetryAttempts = 3;
    private const int DefaultMaxExceptionsBeforeCircuitBreak = 2;
    private const int DefaultMaxRequestsPerMinute = 100;
    private static readonly TimeSpan[] DefaultRetryDelays = [TimeSpan.FromSeconds(1), TimeSpan.FromSeconds(2), TimeSpan.FromSeconds(4)];
    private static readonly AsyncRetryPolicy<HttpResponseMessage> DefaultRetryPolicy =
        PollyExtensions.GetRetryPolicy<HttpResponseMessage>(DefaultMaxRetryAttempts, DefaultRetryDelays, r => !r.IsSuccessStatusCode);
    private static readonly AsyncCircuitBreakerPolicy<HttpResponseMessage> DefaultCircuitBreakerPolicy =
        PollyExtensions.GetCircuitBreakerPolicy<HttpResponseMessage>(DefaultMaxExceptionsBeforeCircuitBreak, TimeSpan.FromMinutes(1), r => !r.IsSuccessStatusCode);
    private static readonly AsyncRateLimitPolicy<HttpResponseMessage> DefaultRateLimitPolicy =
        PollyExtensions.GetRateLimitPolicy<HttpResponseMessage>(DefaultMaxRequestsPerMinute, TimeSpan.FromMinutes(1));

    private readonly AsyncRetryPolicy<HttpResponseMessage> _retryPolicy;
    private readonly AsyncCircuitBreakerPolicy<HttpResponseMessage> _circuitBreakerPolicy;
    private readonly AsyncRateLimitPolicy<HttpResponseMessage> _rateLimitPolicy;
    private readonly SemaphoreSlim _semaphore;
    private readonly ITokenHttpClient _tokenHttpClient;
    private readonly ITokenValidator _tokenValidator;
    private readonly ITokenCache _tokenCache;
    private readonly ILogger<TokenService> _logger;

    public TokenService(
        ITokenHttpClient tokenHttpClient,
        IOptions<TokenServiceOptions> options,
        ITokenCache tokenCache,
        ITokenValidator tokenValidator,
        ILogger<TokenService> logger,
        AsyncRetryPolicy<HttpResponseMessage>? retryPolicy = null,
        AsyncCircuitBreakerPolicy<HttpResponseMessage>? circuitBreakerPolicy = null,
        AsyncRateLimitPolicy<HttpResponseMessage>? rateLimitPolicy = null) {
        if (string.IsNullOrEmpty(options.Value.ClientId))
            throw new ArgumentException("ClientId must be provided", nameof(options));

        _semaphore = new SemaphoreSlim(options.Value.MaxConcurrentRequests);
        _tokenHttpClient = tokenHttpClient;
        _tokenValidator = tokenValidator;
        _tokenCache = tokenCache;
        _logger = logger;

        _retryPolicy = retryPolicy ?? DefaultRetryPolicy;
        _circuitBreakerPolicy = circuitBreakerPolicy ?? DefaultCircuitBreakerPolicy;
        _rateLimitPolicy = rateLimitPolicy ?? DefaultRateLimitPolicy;
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
    /// This method sends a GET request to the OAuth2 token info endpoint to validate the access token.
    /// If the token is valid, it returns a response with the token's expiration time, email, and provider user ID.
    /// If the token is invalid or expired, it returns an empty response.
    /// </remarks>
    public async Task<ValidateAccessTokenResponse> ValidateAccessToken(string accessToken, CancellationToken ct) {
        using IDisposable? accessTokenLogContext = _logger.BeginScope("AccessToken: {AccessToken}", accessToken);

        _logger.LogInformation("ValidateAccessToken");

        ValidateAccessTokenResponse? cachedResponse = _tokenCache.Get(accessToken);
        if (cachedResponse is not null && cachedResponse.IsValid) {
            _logger.LogInformation("ValidateAccessToken - found in cache");
            return cachedResponse;
        }

        _logger.LogInformation("ValidateAccessToken - not found in cache, validating with provider now");

        Context context = new Context().WithLogger(_logger);

        if (!_semaphore.Wait(0, ct)) {
            _logger.LogWarning("ValidateAccessToken - too many outstanding requests");
            throw new InvalidOperationException("The service is too busy to handle the request");
        }

        try {
            HttpResponseMessage response = await Policy.WrapAsync(_rateLimitPolicy, _retryPolicy, _circuitBreakerPolicy)
                .ExecuteAsync((ctx, ct) => _tokenHttpClient.GetTokenInfoAsync(accessToken, ct), context, ct);

            if (!response.IsSuccessStatusCode) {
                _logger.LogWarning("ValidateAccessToken - failed - Status Code {StatusCode}", response.StatusCode);
                return ValidateAccessTokenResponse.Empty;
            }

            string responseContent = await response.Content.ReadAsStringAsync(ct);
            TokenInfoResponse? tokenInfo = Conventions.Deserialize<TokenInfoResponse>(responseContent);

            ValidateAccessTokenResponse validateResponse = _tokenValidator.ValidateTokenInfo(tokenInfo, responseContent);

            if (validateResponse.IsValid)
                _tokenCache.Set(accessToken, validateResponse, TimeSpan.FromSeconds(tokenInfo!.ExpiresIn));

            return validateResponse;
        } finally {
            _ = _semaphore.Release();
        }
    }
}
