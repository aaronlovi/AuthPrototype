namespace AuthPrototype.Models;

public record TokenRefreshRequest(string Email, string AccessToken, string RefreshToken);
