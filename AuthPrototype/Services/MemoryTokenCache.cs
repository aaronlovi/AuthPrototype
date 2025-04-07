using System;
using AuthPrototype.Models;
using Microsoft.Extensions.Caching.Memory;

namespace AuthPrototype.Services;

public class MemoryTokenCache : ITokenCache {
    private static readonly TimeSpan DefaultExpiration = TimeSpan.FromMinutes(5);

    private readonly IMemoryCache _cache;

    public MemoryTokenCache(IMemoryCache cache) {
        _cache = cache;
    }

    public ValidateAccessTokenResponse? Get(string accessToken) {
        _cache.TryGetValue(accessToken, out ValidateAccessTokenResponse? response);
        return response;
    }

    public void Set(string accessToken, ValidateAccessTokenResponse response, TimeSpan? expiration) {
        _cache.Set(accessToken, response, expiration ?? DefaultExpiration);
    }
}
