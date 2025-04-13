using System;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Options;
using OAuthToolkit.Contracts;
using OAuthToolkit.Models;

namespace OAuthToolkit.Services;

/// <summary>
/// Implementation of <see cref="ITokenHttpClient"/> that communicates with Google's OAuth2 token info endpoint.
/// </summary>
internal class TokenHttpClient : ITokenHttpClient {
    private readonly HttpClient _httpClient;
    private readonly string _tokenInfoUriBase;

    /// <summary>
    /// Initializes a new instance of the <see cref="TokenHttpClient"/> class.
    /// </summary>
    /// <param name="httpClient">The HTTP client used to send requests to the token info endpoint.</param>
    /// <param name="options">Configuration options for the token service.</param>
    public TokenHttpClient(HttpClient httpClient, IOptions<TokenServiceOptions> options) {
        _httpClient = httpClient;
        _httpClient.Timeout = TimeSpan.FromSeconds(options.Value.HttpClientTimeoutSeconds);
        _tokenInfoUriBase = options.Value.TokenInfoUriBase;
    }

    /// <summary>
    /// Sends an HTTP request to Google's OAuth2 token info endpoint to validate an access token.
    /// </summary>
    /// <param name="accessToken">The access token to validate.</param>
    /// <param name="ct">A cancellation token that can be used to cancel the operation.</param>
    /// <returns>
    /// The HTTP response from the token info endpoint containing token validation results.
    /// </returns>
    public async Task<HttpResponseMessage> GetTokenInfoAsync(string accessToken, CancellationToken ct) =>
        await _httpClient.GetAsync($"{_tokenInfoUriBase}?access_token={accessToken}", ct);
}
