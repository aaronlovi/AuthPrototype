using System;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using OAuthToolkit.Contracts;
using OAuthToolkit.Models;
using OAuthToolkit.Shared;

namespace OAuthToolkit.Services;

internal class TokenValidator : ITokenValidator {
    private const int MaxLogStringLength = 1000;

    private readonly string _clientId;
    private readonly ILogger<TokenValidator> _logger;

    public TokenValidator(IOptions<TokenServiceOptions> options, ILogger<TokenValidator> logger) {
        _clientId = options.Value.ClientId;
        _logger = logger;
    }

    public ValidateAccessTokenResponse ValidateTokenInfo(TokenInfoResponse? tokenInfo, string rawResponse) {
        if (tokenInfo is null || tokenInfo.ExpiresIn <= 0 || tokenInfo.Aud != _clientId) {
            _logger.LogWarning("ValidateTokenInfo - failed - Invalid raw response: {RawResponse}",
                rawResponse.Truncate(MaxLogStringLength));
            return ValidateAccessTokenResponse.Empty;
        }

        var expirationDateTime = DateTime.UtcNow.AddSeconds(tokenInfo.ExpiresIn);
        return new ValidateAccessTokenResponse(expirationDateTime, tokenInfo.Email, tokenInfo.Sub);
    }
}
