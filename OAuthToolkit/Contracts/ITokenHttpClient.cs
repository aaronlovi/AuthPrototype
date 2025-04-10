using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;

namespace OAuthToolkit.Contracts;

public interface ITokenHttpClient {
    Task<HttpResponseMessage> GetTokenInfoAsync(string accessToken, CancellationToken ct);
}
