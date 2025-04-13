using System;
using System.Net;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Options;
using Moq;
using Moq.Protected;
using OAuthToolkit.Models;
using OAuthToolkit.Services;
using Xunit;

namespace OAuthToolkit.Tests;

public class TokenHttpClientTests {
    private const string TokenInfoUriBase = "https://oauth2.googleapis.com/tokeninfo";
    private const string TokenInfoEndpoint = "https://oauth2.googleapis.com/tokeninfo?access_token=test-token";

    /// <summary>
    /// Verifies that the GetTokenInfoAsync method sends an HTTP GET request to the correct token info endpoint URI
    /// with the provided access token. Ensures that the request is properly constructed and sent exactly once.
    /// </summary>
    [Fact]
    public async Task GetTokenInfoAsync_SendsRequestToCorrectUri() {
        // Arrange
        var mockHttpMessageHandler = new Mock<HttpMessageHandler>();
        _ = mockHttpMessageHandler
            .Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.Is<HttpRequestMessage>(req =>
                    req.Method == HttpMethod.Get &&
                    req.RequestUri!.ToString() == TokenInfoEndpoint),
                ItExpr.IsAny<CancellationToken>())
            .ReturnsAsync(new HttpResponseMessage {
                StatusCode = HttpStatusCode.OK,
                Content = new StringContent("{}")
            });

        using var httpClient = new HttpClient(mockHttpMessageHandler.Object);
        IOptions<TokenServiceOptions> options = Options.Create(new TokenServiceOptions {
            TokenInfoUriBase = TokenInfoUriBase,
            HttpClientTimeoutSeconds = 30
        });

        var tokenHttpClient = new TokenHttpClient(httpClient, options);

        // Act
        HttpResponseMessage response = await tokenHttpClient.GetTokenInfoAsync("test-token", CancellationToken.None);

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        mockHttpMessageHandler.Protected().Verify(
            "SendAsync",
            Times.Once(),
            ItExpr.Is<HttpRequestMessage>(req =>
                req.Method == HttpMethod.Get &&
                req.RequestUri!.ToString() == TokenInfoEndpoint),
            ItExpr.IsAny<CancellationToken>());
    }

    /// <summary>
    /// Tests that GetTokenInfoAsync correctly returns an error response when provided with an invalid access token.
    /// Verifies the response has Unauthorized status code and contains the expected error message.
    /// </summary>
    [Fact]
    public async Task GetTokenInfoAsync_ReturnsErrorResponse_ForInvalidAccessToken() {
        // Arrange
        var mockHttpMessageHandler = new Mock<HttpMessageHandler>();
        _ = mockHttpMessageHandler
            .Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.IsAny<HttpRequestMessage>(),
                ItExpr.IsAny<CancellationToken>())
            .ReturnsAsync(new HttpResponseMessage {
                StatusCode = HttpStatusCode.Unauthorized,
                Content = new StringContent("{\"error\":\"invalid_token\"}")
            });

        using var httpClient = new HttpClient(mockHttpMessageHandler.Object);
        IOptions<TokenServiceOptions> options = Options.Create(new TokenServiceOptions {
            TokenInfoUriBase = TokenInfoUriBase,
            HttpClientTimeoutSeconds = 30
        });

        var tokenHttpClient = new TokenHttpClient(httpClient, options);

        // Act
        HttpResponseMessage response = await tokenHttpClient.GetTokenInfoAsync("invalid-token", CancellationToken.None);

        // Assert
        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
        Assert.Contains("invalid_token", await response.Content.ReadAsStringAsync());
    }

    /// <summary>
    /// Verifies that GetTokenInfoAsync properly propagates a TaskCanceledException when the HTTP request times out.
    /// Confirms the timeout handling behavior when the configured timeout threshold is exceeded.
    /// </summary>
    [Fact]
    public async Task GetTokenInfoAsync_ThrowsTaskCanceledException_OnTimeout() {
        // Arrange
        var mockHttpMessageHandler = new Mock<HttpMessageHandler>();
        _ = mockHttpMessageHandler
            .Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.IsAny<HttpRequestMessage>(),
                ItExpr.IsAny<CancellationToken>())
            .ThrowsAsync(new TaskCanceledException("The request was canceled due to the configured HttpClient.Timeout"));

        using var httpClient = new HttpClient(mockHttpMessageHandler.Object) {
            Timeout = TimeSpan.FromMilliseconds(100)
        };
        IOptions<TokenServiceOptions> options = Options.Create(new TokenServiceOptions {
            TokenInfoUriBase = TokenInfoUriBase,
            HttpClientTimeoutSeconds = 1
        });

        var tokenHttpClient = new TokenHttpClient(httpClient, options);

        // Act & Assert
        _ = await Assert.ThrowsAsync<TaskCanceledException>(() =>
            tokenHttpClient.GetTokenInfoAsync("test-token", CancellationToken.None));
    }

    /// <summary>
    /// Tests that GetTokenInfoAsync correctly uses a custom token info URI when configured.
    /// Ensures that requests are sent to the custom endpoint and verifies the response is processed correctly.
    /// </summary>
    [Fact]
    public async Task GetTokenInfoAsync_UsesCustomTokenInfoUri() {
        // Arrange
        const string customUriBase = "https://custom-oauth2.com/tokeninfo";
        const string customEndpoint = "https://custom-oauth2.com/tokeninfo?access_token=test-token";

        var mockHttpMessageHandler = new Mock<HttpMessageHandler>();
        _ = mockHttpMessageHandler
            .Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.Is<HttpRequestMessage>(req =>
                    req.Method == HttpMethod.Get &&
                    req.RequestUri!.ToString() == customEndpoint),
                ItExpr.IsAny<CancellationToken>())
            .ReturnsAsync(new HttpResponseMessage {
                StatusCode = HttpStatusCode.OK,
                Content = new StringContent("{}")
            });

        using var httpClient = new HttpClient(mockHttpMessageHandler.Object);
        IOptions<TokenServiceOptions> options = Options.Create(new TokenServiceOptions {
            TokenInfoUriBase = customUriBase,
            HttpClientTimeoutSeconds = 30
        });

        var tokenHttpClient = new TokenHttpClient(httpClient, options);

        // Act
        HttpResponseMessage response = await tokenHttpClient.GetTokenInfoAsync("test-token", CancellationToken.None);

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        mockHttpMessageHandler.Protected().Verify(
            "SendAsync",
            Times.Once(),
            ItExpr.Is<HttpRequestMessage>(req =>
                req.Method == HttpMethod.Get &&
                req.RequestUri!.ToString() == customEndpoint),
            ItExpr.IsAny<CancellationToken>());
    }

    /// <summary>
    /// Verifies that GetTokenInfoAsync properly propagates any HttpRequestException that occurs during the HTTP request.
    /// Ensures network errors and other HTTP-related exceptions are not caught and suppressed by the client.
    /// </summary>
    [Fact]
    public async Task GetTokenInfoAsync_PropagatesHttpRequestException() {
        // Arrange
        var mockHttpMessageHandler = new Mock<HttpMessageHandler>();
        _ = mockHttpMessageHandler
            .Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.IsAny<HttpRequestMessage>(),
                ItExpr.IsAny<CancellationToken>())
            .ThrowsAsync(new HttpRequestException("Network error"));

        using var httpClient = new HttpClient(mockHttpMessageHandler.Object);
        IOptions<TokenServiceOptions> options = Options.Create(new TokenServiceOptions {
            TokenInfoUriBase = TokenInfoUriBase,
            HttpClientTimeoutSeconds = 30
        });

        var tokenHttpClient = new TokenHttpClient(httpClient, options);

        // Act & Assert
        HttpRequestException exception = await Assert.ThrowsAsync<HttpRequestException>(() =>
            tokenHttpClient.GetTokenInfoAsync("test-token", CancellationToken.None));
        Assert.Equal("Network error", exception.Message);
    }
}
