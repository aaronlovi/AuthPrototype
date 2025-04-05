namespace AuthPrototype.Models;

public record OAuthTokenRequest(string Name, string Email, string AccessToken, string RefreshToken);