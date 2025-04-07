using System;
using AuthPrototype.Models;
using AuthPrototype.Utilities;
using Microsoft.Extensions.Logging;

namespace AuthPrototype.Services;

public class TokenValidator {
    private const int MaxLogStringLength = 1000;

    private readonly string _clientId;

    public TokenValidator(string clientId) {
        _clientId = clientId;
    }

    public ValidateAccessTokenResponse ValidateTokenInfo(TokenInfoResponse? tokenInfo, string rawResponse, ILogger logger) {
        if (tokenInfo is null || tokenInfo.ExpiresIn <= 0 || tokenInfo.Aud != _clientId) {
            logger.LogWarning("ValidateAccessToken - failed - Invalid raw response: {RawResponse}",
                rawResponse.Truncate(MaxLogStringLength));
            return ValidateAccessTokenResponse.Empty;
        }

        var expirationDateTime = DateTime.UtcNow.AddSeconds(tokenInfo.ExpiresIn);
        return new ValidateAccessTokenResponse(expirationDateTime, tokenInfo.Email, tokenInfo.Sub);
    }
}
