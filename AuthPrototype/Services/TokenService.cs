using System;
using System.Net.Http;
using System.Threading.Tasks;
using AuthPrototype.Models;
using AuthPrototype.Utilities;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace AuthPrototype.Services;

public class TokenService
{
    private const int MaxLogStringLength = 1000;

    private readonly HttpClient _httpClient;
    private readonly string _clientId;
    private readonly ITokenCache _tokenCache;
    private readonly ILogger<TokenService> _logger;

    public TokenService(HttpClient httpClient, IConfiguration config, ITokenCache tokenCache, ILogger<TokenService> logger)
    {
        _httpClient = httpClient;
        _clientId = config["CLIENT_ID"] ?? throw new InvalidOperationException("CLIENT_ID not set");
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

        _logger.LogInformation("ValidateAccessToken - to Google");

        var response = await _httpClient.GetAsync($"https://oauth2.googleapis.com/tokeninfo?access_token={accessToken}");
        if (!response.IsSuccessStatusCode) {
            _logger.LogWarning("ValidateAccessToken - failed - Status Code {StatusCode}", response.StatusCode);
            return ValidateAccessTokenResponse.Empty;
        }

        var responseContent = await response.Content.ReadAsStringAsync();
        var tokenInfo = Conventions.Deserialize<TokenInfoResponse>(responseContent);

        if (tokenInfo is null || tokenInfo.ExpiresIn <= 0 || tokenInfo.Aud != _clientId) {

            _logger.LogWarning("ValidateAccessToken - failed - Invalid raw response: {RawResponse}",
                responseContent.Truncate(MaxLogStringLength));
            return ValidateAccessTokenResponse.Empty;
        }

        var expirationDateTime = DateTime.UtcNow.AddSeconds(tokenInfo.ExpiresIn);
        var validateResponse = new ValidateAccessTokenResponse(expirationDateTime, tokenInfo.Email, tokenInfo.Sub);

        _tokenCache.Set(accessToken, validateResponse, TimeSpan.FromSeconds(tokenInfo.ExpiresIn));

        return validateResponse;
    }
}
