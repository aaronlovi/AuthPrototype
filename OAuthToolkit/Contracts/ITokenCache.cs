using System;
using OAuthToolkit.Models;

namespace OAuthToolkit.Contracts;

public interface ITokenCache {
    ValidateAccessTokenResponse? Get(string accessToken);
    void Set(string accessToken, ValidateAccessTokenResponse response, TimeSpan? expiration);
}
