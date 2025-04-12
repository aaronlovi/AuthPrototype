using System;
using OAuthToolkit.Models;

namespace OAuthToolkit.Contracts;

/// <summary>
/// Defines a contract for caching and retrieving access token validation results.
/// Implementations of this interface provide a caching mechanism to store token validation
/// responses and reduce the need for repeated validation of the same token.
/// </summary>
public interface ITokenCache {
    /// <summary>
    /// Retrieves a cached token validation response for the specified access token.
    /// </summary>
    /// <param name="accessToken">The access token to retrieve validation information for.</param>
    /// <returns>
    /// The cached validation response if found; otherwise, null.
    /// </returns>
    ValidateAccessTokenResponse? Get(string accessToken);

    /// <summary>
    /// Caches a token validation response for the specified access token.
    /// </summary>
    /// <param name="accessToken">The access token to cache validation information for.</param>
    /// <param name="response">The validation response to cache.</param>
    /// <param name="expiration">
    /// Optional. The time period after which the cached entry should expire.
    /// If null, the implementation should use a default expiration time.
    /// </param>
    void Set(string accessToken, ValidateAccessTokenResponse response, TimeSpan? expiration);
}
