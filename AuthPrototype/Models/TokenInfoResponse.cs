using System.Text.Json.Serialization;

namespace AuthPrototype.Models;

public record TokenInfoResponse(
    [property: JsonPropertyName("expires_in")] int ExpiresIn,
    string Aud,
    string Sub,
    string Email);
