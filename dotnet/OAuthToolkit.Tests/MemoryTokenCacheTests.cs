using System;
using Microsoft.Extensions.Caching.Memory;
using OAuthToolkit.Models;
using OAuthToolkit.Services;
using Xunit;

namespace OAuthToolkit.Tests;

public class MemoryTokenCacheTests {
    private const string _testAccessToken = "test-access-token";

    private readonly MemoryCache _cache;
    private readonly MemoryTokenCache _tokenCache;
    private readonly ValidateAccessTokenResponse _testResponse;

    public MemoryTokenCacheTests() {
        _cache = new MemoryCache(new MemoryCacheOptions());
        _tokenCache = new MemoryTokenCache(_cache);
        _testResponse = new ValidateAccessTokenResponse(
            DateTime.UtcNow.AddHours(1),
            "test@example.com",
            "test-user-123");
    }

    /// <summary>
    /// Verifies that the Get method returns the cached response when a token exists in the cache.
    /// </summary>
    [Fact]
    public void Get_WhenTokenExists_ReturnsResponse() {
        // Arrange
        _ = _cache.Set(_testAccessToken, _testResponse);

        // Act
        ValidateAccessTokenResponse? result = _tokenCache.Get(_testAccessToken);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(_testResponse, result);
    }

    /// <summary>
    /// Verifies that the Get method returns null when a token does not exist in the cache.
    /// </summary>
    [Fact]
    public void Get_WhenTokenDoesNotExist_ReturnsNull() {
        // Act
        ValidateAccessTokenResponse? result = _tokenCache.Get(_testAccessToken);

        // Assert
        Assert.Null(result);
    }

    /// <summary>
    /// Tests that Set method correctly uses the explicitly provided expiration time when caching a token.
    /// </summary>
    [Fact]
    public void Set_WithExplicitExpiration_StoresWithCorrectExpiration() {
        // Arrange
        var expiration = TimeSpan.FromMinutes(10);

        // Act
        _tokenCache.Set(_testAccessToken, _testResponse, expiration);

        // Assert
        ValidateAccessTokenResponse? cachedItem = _cache.Get<ValidateAccessTokenResponse>(_testAccessToken);
        Assert.NotNull(cachedItem);
        Assert.Equal(_testResponse, cachedItem);
    }

    /// <summary>
    /// Confirms that the Set method uses the default 5-minute expiration when no expiration is specified.
    /// </summary>
    [Fact]
    public void Set_WithNullExpiration_UsesDefaultExpiration() {
        // Act
        _tokenCache.Set(_testAccessToken, _testResponse, null);

        // Assert
        ValidateAccessTokenResponse? cachedItem = _cache.Get<ValidateAccessTokenResponse>(_testAccessToken);
        Assert.NotNull(cachedItem);
        Assert.Equal(_testResponse, cachedItem);
    }

    /// <summary>
    /// Verifies that the constructor throws ArgumentNullException when passed a null memory cache.
    /// </summary>
    [Fact]
    public void Construct_WithNullMemoryCache_ThrowsArgumentNullException() =>
        // Act & Assert
        _ = Assert.Throws<ArgumentNullException>(() => new MemoryTokenCache(null!));
}
