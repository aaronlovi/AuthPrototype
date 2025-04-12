using System;
using Microsoft.Extensions.Caching.Memory;
using OAuthToolkit.Contracts;
using OAuthToolkit.Models;

namespace OAuthToolkit.Services;

/// <summary>
/// An in-memory implementation of <see cref="ITokenCache"/> that stores token validation
/// responses using <see cref="IMemoryCache"/>.
/// </summary>
internal class MemoryTokenCache : ITokenCache {
    /// <summary>
    /// The default time period for which cached entries will be kept if no specific expiration is provided.
    /// </summary>
    private static readonly TimeSpan DefaultExpiration = TimeSpan.FromMinutes(5);

    /// <summary>
    /// The underlying memory cache used to store token validation responses.
    /// </summary>
    private readonly IMemoryCache _cache;

    /// <summary>
    /// Initializes a new instance of the <see cref="MemoryTokenCache"/> class.
    /// </summary>
    /// <param name="cache">The memory cache instance to use for token storage.</param>
    public MemoryTokenCache(IMemoryCache cache) {
        ArgumentNullException.ThrowIfNull(cache);
        _cache = cache;
    }

    /// <summary>
    /// Retrieves a token validation response from the cache.
    /// </summary>
    /// <param name="accessToken">The access token to look up.</param>
    /// <returns>
    /// The cached validation response if found; otherwise, null.
    /// </returns>
    public ValidateAccessTokenResponse? Get(string accessToken) {
        _ = _cache.TryGetValue(accessToken, out ValidateAccessTokenResponse? response);
        return response;
    }

    /// <summary>
    /// Stores a token validation response in the cache.
    /// </summary>
    /// <param name="accessToken">The access token to cache validation information for.</param>
    /// <param name="response">The validation response to cache.</param>
    /// <param name="expiration">
    /// Optional. The time period after which the cached entry should expire.
    /// If null, the default expiration of 5 minutes will be used.
    /// </param>
    public void Set(string accessToken, ValidateAccessTokenResponse response, TimeSpan? expiration) =>
        _cache.Set(accessToken, response, expiration ?? DefaultExpiration);
}
