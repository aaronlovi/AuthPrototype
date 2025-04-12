using System;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using OAuthToolkit.Contracts;
using OAuthToolkit.Models;
using OAuthToolkit.Shared;

namespace OAuthToolkit.Services;

/// <summary>
/// Implementation of <see cref="ITokenValidator"/> that validates token information from the OAuth2 provider.
/// </summary>
internal class TokenValidator : ITokenValidator {
    /// <summary>
    /// Maximum length of raw response to include in log messages.
    /// </summary>
    private const int MaxLogStringLength = 1000;

    private readonly string _clientId;
    private readonly ILogger<TokenValidator> _logger;

    /// <summary>
    /// Initializes a new instance of the <see cref="TokenValidator"/> class.
    /// </summary>
    /// <param name="options">Configuration options for the token service.</param>
    /// <param name="logger">Logger used to record validation events and errors.</param>
    public TokenValidator(IOptions<TokenServiceOptions> options, ILogger<TokenValidator> logger) {
        _clientId = options.Value.ClientId;
        _logger = logger;
    }

    /// <summary>
    /// Validates token information received from an OAuth2 provider.
    /// </summary>
    /// <param name="tokenInfo">The deserialized token information response from the OAuth2 provider.</param>
    /// <param name="rawResponse">The raw response string received from the OAuth2 provider for logging purposes.</param>
    /// <returns>
    /// A <see cref="ValidateAccessTokenResponse"/> containing validation results and user information
    /// if the token is valid; otherwise, returns <see cref="ValidateAccessTokenResponse.Empty"/>.
    /// </returns>
    /// <remarks>
    /// The token is considered valid if:
    /// - The token information is not null
    /// - The token has not expired (ExpiresIn > 0)
    /// - The token's audience (Aud) matches the configured client ID
    /// </remarks>
    public ValidateAccessTokenResponse ValidateTokenInfo(TokenInfoResponse? tokenInfo, string rawResponse) {
        if (tokenInfo is null || tokenInfo.ExpiresIn <= 0 || tokenInfo.Aud != _clientId) {
            _logger.LogWarning("ValidateTokenInfo - failed - Invalid raw response: {RawResponse}",
                rawResponse.Truncate(MaxLogStringLength));
            return ValidateAccessTokenResponse.Empty;
        }

        DateTime expirationDateTime = DateTime.UtcNow.AddSeconds(tokenInfo.ExpiresIn);
        return new ValidateAccessTokenResponse(expirationDateTime, tokenInfo.Email, tokenInfo.Sub);
    }
}
