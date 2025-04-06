using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text.Json;
using System.Threading.Tasks;
using AuthPrototype.Models;
using Microsoft.Extensions.Configuration;

namespace AuthPrototype.Services;

public class TokenService
{
    private readonly HttpClient _httpClient;
    private readonly string _clientId;

    public TokenService(HttpClient httpClient, IConfiguration config)
    {
        _httpClient = httpClient;
        _clientId = config["CLIENT_ID"] ?? throw new InvalidOperationException("CLIENT_ID not set");
    }

    public async Task<ValidateAccessTokenResponse> ValidateAccessTokenAsync(string accessToken)
    {
        var response = await _httpClient.GetAsync($"https://oauth2.googleapis.com/tokeninfo?access_token={accessToken}");
        if (!response.IsSuccessStatusCode)
            return ValidateAccessTokenResponse.Invalid;

        var responseContent = await response.Content.ReadAsStringAsync();
        var tokenInfo = Conventions.Deserialize<TokenInfoResponse>(responseContent);

        if (tokenInfo == null || tokenInfo.ExpiresIn <= 0 || tokenInfo.Aud != _clientId)
            return ValidateAccessTokenResponse.Invalid;

        var expirationDateTime = DateTime.UtcNow.AddSeconds(tokenInfo.ExpiresIn);
        return new ValidateAccessTokenResponse(true, expirationDateTime);
    }
}
