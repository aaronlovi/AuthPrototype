using System.Threading;
using System.Threading.Tasks;
using OAuthToolkit.Models;

namespace OAuthToolkit.Contracts;

/// <summary>
/// Defines a contract for validating OAuth2 access tokens.
/// </summary>
/// <remarks>
/// Implementations of this interface handle the validation of access tokens by verifying them
/// with an OAuth2 provider and managing token caching to improve performance.
/// </remarks>
public interface ITokenService {
    /// <summary>
    /// Validates an OAuth2 access token by checking it with the token provider.
    /// </summary>
    /// <param name="accessToken">The access token to validate.</param>
    /// <param name="ct">A cancellation token that can be used to cancel the operation.</param>
    /// <returns>
    /// A <see cref="ValidateAccessTokenResponse"/> containing the token's validation status and user information
    /// if the token is valid; otherwise, returns <see cref="ValidateAccessTokenResponse.Empty"/>.
    /// </returns>
    /// <remarks>
    /// This method will first check if the token is available in the cache. If found and still valid,
    /// it returns the cached information. Otherwise, it contacts the OAuth2 provider to validate the token.
    /// </remarks>
    Task<ValidateAccessTokenResponse> ValidateAccessToken(string accessToken, CancellationToken ct);
}
