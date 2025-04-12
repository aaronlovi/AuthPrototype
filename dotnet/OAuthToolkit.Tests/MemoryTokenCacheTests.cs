using System;
using Microsoft.Extensions.Caching.Memory;
using Moq;
using OAuthToolkit.Models;
using OAuthToolkit.Services;
using Xunit;

namespace OAuthToolkit.Tests;

public class MemoryTokenCacheTests {
    private const string _testAccessToken = "test-access-token";

    private readonly Mock<IMemoryCache> _mockCache; // Use a mock for IMemoryCache
    private readonly MemoryTokenCache _tokenCache;
    private readonly ValidateAccessTokenResponse _testResponse;

    public MemoryTokenCacheTests() {
        _mockCache = new Mock<IMemoryCache>();
        _tokenCache = new MemoryTokenCache(_mockCache.Object);
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
        object? cachedValue = _testResponse;
        _ = _mockCache
            .Setup(cache => cache.TryGetValue(It.IsAny<object>(), out cachedValue))
            .Returns(true);

        // Act
        ValidateAccessTokenResponse? result = _tokenCache.Get(_testAccessToken);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(_testResponse, result);

        // Modified verification to match the setup
        _mockCache.Verify(cache => cache.TryGetValue(It.IsAny<object>(), out It.Ref<object?>.IsAny), Times.Once);
    }

    /// <summary>
    /// Verifies that the Get method returns null when a token does not exist in the cache.
    /// </summary>
    [Fact]
    public void Get_WhenTokenDoesNotExist_ReturnsEmpty() {
        // Arrange
        object? cachedValue = null;
        _ = _mockCache
            .Setup(cache => cache.TryGetValue(It.IsAny<object>(), out cachedValue))
            .Returns(false);

        // Act
        ValidateAccessTokenResponse? result = _tokenCache.Get(_testAccessToken);

        // Assert
        Assert.True(result!.IsInvalid);
        _mockCache.Verify(cache => cache.TryGetValue(It.IsAny<object>(), out It.Ref<object?>.IsAny), Times.Once);
    }

    /// <summary>
    /// Verifies that the Set method stores the response in the cache with the correct expiration.
    /// </summary>
    [Fact]
    public void Set_StoresResponseInCache() {
        // Arrange
        var cacheEntryMock = new Mock<ICacheEntry>();
        _ = _mockCache
            .Setup(cache => cache.CreateEntry(_testAccessToken))
            .Returns(cacheEntryMock.Object);

        // Act
        _tokenCache.Set(_testAccessToken, _testResponse, TimeSpan.FromMinutes(10));

        // Assert
        _mockCache.Verify(cache => cache.CreateEntry(_testAccessToken), Times.Once);
        cacheEntryMock.VerifySet(entry => entry.AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(10), Times.Once);
    }

    /// <summary>
    /// Verifies that the constructor throws ArgumentNullException when passed a null memory cache.
    /// </summary>
    [Fact]
    public void Construct_WithNullMemoryCache_ThrowsArgumentNullException() =>
        // Act & Assert
        _ = Assert.Throws<ArgumentNullException>(() => new MemoryTokenCache(null!));
    /// <summary>
    /// Verifies that the Set method uses the default expiration when none is provided.
    /// </summary>
    [Fact]
    public void Set_WithNullExpiration_UsesDefaultExpiration() {
        // Arrange
        var cacheEntryMock = new Mock<ICacheEntry>();
        _ = _mockCache
            .Setup(cache => cache.CreateEntry(_testAccessToken))
            .Returns(cacheEntryMock.Object);

        // Act
        _tokenCache.Set(_testAccessToken, _testResponse, null);

        // Assert
        _mockCache.Verify(cache => cache.CreateEntry(_testAccessToken), Times.Once);
        cacheEntryMock.VerifySet(entry => entry.AbsoluteExpirationRelativeToNow = MemoryTokenCache.DefaultExpiration, Times.Once);
    }

    /// <summary>
    /// Verifies that the Get method handles null access tokens appropriately.
    /// </summary>
    [Fact]
    public void Get_WithNullAccessToken_HandlesGracefully() {
        // Arrange
        string? nullToken = null;

        // Act & Assert
        ValidateAccessTokenResponse? res = _tokenCache.Get(nullToken!);

        // Assert
        Assert.True(res!.IsInvalid);
    }

    /// <summary>
    /// Verifies that the Set method handles null access tokens appropriately.
    /// </summary>
    [Fact]
    public void Set_WithNullAccessToken_HandlesGracefully() {
        // Arrange
        string? nullToken = null;

        // Act & Assert
        ArgumentNullException exception = Assert.Throws<ArgumentNullException>(() =>
            _tokenCache.Set(nullToken!, _testResponse, TimeSpan.FromMinutes(10)));
        Assert.Equal("accessToken", exception.ParamName);
    }

    /// <summary>
    /// Verifies that the Set method handles null response appropriately.
    /// </summary>
    [Fact]
    public void Set_WithNullResponse_HandlesGracefully() {
        // Arrange
        ValidateAccessTokenResponse? nullResponse = null;

        // Act & Assert
        ArgumentNullException exception = Assert.Throws<ArgumentNullException>(() =>
            _tokenCache.Set(_testAccessToken, nullResponse!, TimeSpan.FromMinutes(10)));
        Assert.Equal("response", exception.ParamName);
    }

    /// <summary>
    /// Verifies that the Set method handles negative expiration times appropriately.
    /// </summary>
    [Fact]
    public void Set_WithNegativeExpiration_HandlesGracefully() {
        // Arrange
        var cacheEntryMock = new Mock<ICacheEntry>();
        _ = _mockCache
            .Setup(cache => cache.CreateEntry(_testAccessToken))
            .Returns(cacheEntryMock.Object);

        // Act
        _tokenCache.Set(_testAccessToken, _testResponse, TimeSpan.FromMinutes(-10));

        // Assert - The implementation should allow negative expiration (which would effectively expire immediately)
        _mockCache.Verify(cache => cache.CreateEntry(_testAccessToken), Times.Once);
        cacheEntryMock.VerifySet(entry => entry.AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(-10), Times.Once);
    }

    /// <summary>
    /// Verifies that Set can store the ValidateAccessTokenResponse.Empty instance.
    /// </summary>
    [Fact]
    public void Set_WithEmptyResponse_StoresCorrectly() {
        // Arrange
        var cacheEntryMock = new Mock<ICacheEntry>();
        _ = _mockCache
            .Setup(cache => cache.CreateEntry(_testAccessToken))
            .Returns(cacheEntryMock.Object);

        // Act
        _tokenCache.Set(_testAccessToken, ValidateAccessTokenResponse.Empty, TimeSpan.FromMinutes(10));

        // Assert
        _mockCache.Verify(cache => cache.CreateEntry(_testAccessToken), Times.Once);
        cacheEntryMock.VerifySet(entry => entry.Value = ValidateAccessTokenResponse.Empty, Times.Once);
    }
}
