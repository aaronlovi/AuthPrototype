using System.Net.Http;
using System.Threading.Tasks;

namespace AuthPrototype.Services;

public class TokenHttpClient {
    private readonly HttpClient _httpClient;

    public TokenHttpClient(HttpClient httpClient) {
        _httpClient = httpClient;
    }

    public async Task<HttpResponseMessage> GetTokenInfoAsync(string accessToken) =>
        await _httpClient.GetAsync($"https://oauth2.googleapis.com/tokeninfo?access_token={accessToken}");
}
