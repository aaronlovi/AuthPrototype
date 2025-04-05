using System.Collections.Generic;
using System.Net.Http;
using System.Text.Json;
using System.Threading.Tasks;
using AuthPrototype.Models;

namespace AuthPrototype.Services;

public class TokenService
{
    private readonly HttpClient _httpClient;

    public TokenService(HttpClient httpClient)
    {
        _httpClient = httpClient;
    }

    public async Task<(string AccessToken, string RefreshToken)> RefreshTokenAsync(string refreshToken)
    {
        var requestContent = new FormUrlEncodedContent(
        [
            new KeyValuePair<string, string>("client_id", "your-client-id"),
            new KeyValuePair<string, string>("client_secret", "your-client-secret"),
            new KeyValuePair<string, string>("refresh_token", refreshToken),
            new KeyValuePair<string, string>("grant_type", "refresh_token")
        ]);

        var response = await _httpClient.PostAsync("https://oauth2.googleapis.com/token", requestContent);
        response.EnsureSuccessStatusCode();

        var responseContent = await response.Content.ReadAsStringAsync();
        var tokenResponse = JsonSerializer.Deserialize<TokenResponse>(responseContent);

        if (tokenResponse == null || string.IsNullOrEmpty(tokenResponse.AccessToken) || string.IsNullOrEmpty(tokenResponse.RefreshToken))
            throw new HttpRequestException("Failed to refresh token. Invalid response from OAuth provider.");

        return (tokenResponse.AccessToken, tokenResponse.RefreshToken);
    }

    public async Task<bool> ValidateAccessTokenAsync(string accessToken)
    {
        var response = await _httpClient.GetAsync($"https://oauth2.googleapis.com/tokeninfo?access_token={accessToken}");
        return response.IsSuccessStatusCode;
    }
}
