using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;

namespace OAuthToolkit.Contracts;

/// <summary>
/// Defines a contract for making HTTP requests to OAuth2 token endpoints.
/// </summary>
public interface ITokenHttpClient {
    /// <summary>
    /// Sends an HTTP request to the OAuth2 provider's token info endpoint to validate an access token.
    /// </summary>
    /// <param name="accessToken">The access token to validate.</param>
    /// <param name="ct">A cancellation token that can be used to cancel the operation.</param>
    /// <returns>
    /// The HTTP response from the token info endpoint containing token validation results.
    /// </returns>
    Task<HttpResponseMessage> GetTokenInfoAsync(string accessToken, CancellationToken ct);
}
