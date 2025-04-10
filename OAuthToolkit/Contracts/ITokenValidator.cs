using OAuthToolkit.Models;

namespace OAuthToolkit.Contracts;

public interface ITokenValidator {
    ValidateAccessTokenResponse ValidateTokenInfo(TokenInfoResponse? tokenInfo, string rawResponse);
}
