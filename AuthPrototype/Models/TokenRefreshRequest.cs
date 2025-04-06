using System;

namespace AuthPrototype.Models;

public record TokenRefreshRequest(string Email, string AccessToken);

public record TokenRefreshResponse(string AccessToken, DateTime ExpirationTime);
