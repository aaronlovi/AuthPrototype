using System;

namespace OAuthToolkit.Models;

public record ValidateAccessTokenResponse(DateTime? ExpirationTime, string Email, string ProviderUserId)
{
    public static readonly ValidateAccessTokenResponse Empty = new(null, string.Empty, string.Empty);

    public bool IsValid => ExpirationTime is not null && !string.IsNullOrWhiteSpace(Email) && !string.IsNullOrWhiteSpace(ProviderUserId);
    public bool IsInvalid => !IsValid;
}
