using System;

namespace OAuthToolkit.Models;

/// <summary>
/// Represents the result of an access token validation operation.
/// Contains information about the token's validity, expiration, and associated user details.
/// </summary>
/// <param name="ExpirationTime">The time when the token expires, or null if not applicable.</param>
/// <param name="Email">The email associated with the token.</param>
/// <param name="ProviderUserId">The user identifier from the authentication provider.</param>
public record ValidateAccessTokenResponse(DateTime? ExpirationTime, string Email, string ProviderUserId) {
    /// <summary>
    /// A pre-defined empty response instance for cases where a token is invalid or validation fails.
    /// </summary>
    public static readonly ValidateAccessTokenResponse Empty = new(null, string.Empty, string.Empty);

    /// <summary>
    /// Gets a value indicating whether this token validation response represents a valid token.
    /// A token is considered valid when it has an expiration time and contains user identification information.
    /// </summary>
    public bool IsValid => ExpirationTime is not null && !string.IsNullOrWhiteSpace(Email) && !string.IsNullOrWhiteSpace(ProviderUserId);

    /// <summary>
    /// Gets a value indicating whether this token validation response represents an invalid token.
    /// </summary>
    public bool IsInvalid => !IsValid;
}
