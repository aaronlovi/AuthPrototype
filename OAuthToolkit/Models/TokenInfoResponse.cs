using System.Text.Json.Serialization;

namespace OAuthToolkit.Models;

/// <summary>
/// Represents the response received from an OAuth2 token info endpoint.
/// </summary>
/// <param name="ExpiresIn">The number of seconds until the token expires.</param>
/// <param name="Aud">The audience (client ID) for which the token was issued.</param>
/// <param name="Sub">The subject identifier, typically the user's unique ID from the provider.</param>
/// <param name="Email">The email address associated with the authenticated user.</param>
public record TokenInfoResponse(
    [property: JsonPropertyName("expires_in")] int ExpiresIn,
    string Aud,
    string Sub,
    string Email);
