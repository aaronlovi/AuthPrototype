using System;
using Microsoft.Extensions.Caching.Memory;
using Moq;
using OAuthToolkit.Models;
using OAuthToolkit.Services;
using Xunit;

namespace OAuthToolkit.Tests
{
    public class MemoryTokenCacheTests
    {
        private readonly Mock<IMemoryCache> _mockCache;
        private readonly OAuthToolkit.Services.MemoryTokenCache _tokenCache;
        private readonly string _testAccessToken = "test-access-token";
        private readonly ValidateAccessTokenResponse _testResponse;

        public MemoryTokenCacheTests()
        {
            _mockCache = new Mock<IMemoryCache>();
            _tokenCache = new OAuthToolkit.Services.MemoryTokenCache(_mockCache.Object);
            _testResponse = new ValidateAccessTokenResponse(
                DateTime.UtcNow.AddHours(1), 
                "test@example.com", 
                "test-user-123");
        }

        [Fact]
        public void Get_WhenTokenExists_ReturnsResponse()
        {
            // Arrange
            object expectedResponse = _testResponse;
            _mockCache.Setup(m => m.TryGetValue(_testAccessToken, out expectedResponse))
                .Returns(true);

            // Act
            var result = _tokenCache.Get(_testAccessToken);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(_testResponse, result);
            _mockCache.Verify(m => m.TryGetValue(_testAccessToken, out It.Ref<object>.IsAny), Times.Once);
        }

        [Fact]
        public void Get_WhenTokenDoesNotExist_ReturnsNull()
        {
            // Arrange
            object expectedResponse = null;
            _mockCache.Setup(m => m.TryGetValue(_testAccessToken, out expectedResponse))
                .Returns(false);

            // Act
            var result = _tokenCache.Get(_testAccessToken);

            // Assert
            Assert.Null(result);
            _mockCache.Verify(m => m.TryGetValue(_testAccessToken, out It.Ref<object>.IsAny), Times.Once);
        }

        [Fact]
        public void Set_WithExplicitExpiration_StoresWithCorrectExpiration()
        {
            // Arrange
            var expiration = TimeSpan.FromMinutes(10);
            
            // Act
            _tokenCache.Set(_testAccessToken, _testResponse, expiration);
            
            // Assert
            _mockCache.Verify(m => m.Set(
                _testAccessToken,
                _testResponse,
                It.Is<TimeSpan>(ts => ts == expiration)), 
                Times.Once);
        }

        [Fact]
        public void Set_WithNullExpiration_UsesDefaultExpiration()
        {
            // Arrange - default expiration is 5 minutes per the implementation
            TimeSpan defaultExpiration = TimeSpan.FromMinutes(5);
            
            // Act
            _tokenCache.Set(_testAccessToken, _testResponse, null);
            
            // Assert
            _mockCache.Verify(m => m.Set(
                _testAccessToken,
                _testResponse,
                It.Is<TimeSpan>(ts => ts == defaultExpiration)), 
                Times.Once);
        }

        [Fact]
        public void Construct_WithNullMemoryCache_ThrowsArgumentNullException()
        {
            // Act & Assert
            Assert.Throws<ArgumentNullException>(() => new MemoryTokenCache(null));
        }
    }
}
