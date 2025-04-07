using System;
using AuthPrototype.Models;

namespace AuthPrototype.Services;

public interface ITokenCache {
    ValidateAccessTokenResponse? Get(string accessToken);
    void Set(string accessToken, ValidateAccessTokenResponse response, TimeSpan? expiration);
}
