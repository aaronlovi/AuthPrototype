using System.Threading;
using System.Threading.Tasks;
using OAuthToolkit.Models;

namespace OAuthToolkit.Contracts;

public interface ITokenService {
    Task<ValidateAccessTokenResponse> ValidateAccessToken(string accessToken, CancellationToken ct);
}
