using OAuthToolkit.Models;

namespace OAuthToolkit.Contracts;

/// <summary>
/// Defines a contract for validating OAuth2 token information responses.
/// </summary>
public interface ITokenValidator {
    /// <summary>
    /// Validates token information received from an OAuth2 provider.
    /// </summary>
    /// <param name="tokenInfo">The deserialized token information response from the OAuth2 provider.</param>
    /// <param name="rawResponse">The raw response string received from the OAuth2 provider for logging purposes.</param>
    /// <returns>
    /// A <see cref="ValidateAccessTokenResponse"/> containing validation results and user information
    /// if the token is valid; otherwise, returns <see cref="ValidateAccessTokenResponse.Empty"/>.
    /// </returns>
    ValidateAccessTokenResponse ValidateTokenInfo(TokenInfoResponse? tokenInfo, string rawResponse);
}
