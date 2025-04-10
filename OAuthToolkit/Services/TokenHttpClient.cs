using System;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Options;
using OAuthToolkit.Contracts;
using OAuthToolkit.Models;

namespace OAuthToolkit.Services;

internal class TokenHttpClient : ITokenHttpClient {
    private readonly HttpClient _httpClient;

    public TokenHttpClient(HttpClient httpClient, IOptions<TokenServiceOptions> options) {
        _httpClient = httpClient;
        _httpClient.Timeout = TimeSpan.FromSeconds(options.Value.HttpClientTimeoutSeconds);
    }

    public async Task<HttpResponseMessage> GetTokenInfoAsync(string accessToken, CancellationToken ct) =>
        await _httpClient.GetAsync($"https://oauth2.googleapis.com/tokeninfo?access_token={accessToken}", ct);
}
